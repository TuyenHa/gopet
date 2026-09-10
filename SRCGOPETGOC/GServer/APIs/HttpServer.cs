using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
[assembly: ApiController]
namespace Gopet.APIs
{
    
    public class HttpServer
    {
        private WebApplication Application { get;  }


        public int Port { get; }

        public HttpServer(int port) : this()
        {
            Port = port;
        }

        protected HttpServer()
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            builder.Services.AddControllers();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gopet API", Description = "Documention of GopetServer", Version = "v1" });
            });



            Application = builder.Build();


            // KHÔNG UseHttpsRedirection: chỉ lắng nghe HTTP, bật cái này sẽ
            // chuyển hướng sang cổng HTTPS không tồn tại.

            // Kiểm tra API key phải đứng TRƯỚC mọi thứ định tuyến, nếu không
            // request vẫn tới được controller.
            var apiKey = ServerSetting.instance.apiKey;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Application.UseGopetApiKey(apiKey);
            }

            Application.UseAuthorization();

            Application.MapControllers();

            Application.UseRouting();

            Application.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Application.UseSwagger();

            Application.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GopetServer API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        /// <summary>
        /// Địa chỉ lắng nghe. Mặc định loopback — API này có quyền tắt máy chủ
        /// và tạo vật phẩm, không được nghe trên mạng nếu chưa bật xác thực.
        /// </summary>
        public string BindAddress { get; } = ServerSetting.instance.httpBindAddress;

        public void Start()
        {
            var apiKey = ServerSetting.instance.apiKey;
            var hasKey = !string.IsNullOrWhiteSpace(apiKey);
            var isLoopback = BindAddress is "127.0.0.1" or "localhost" or "::1";

            // Fail closed: mở ra ngoài loopback mà không có key là cấu hình
            // nguy hiểm nhất có thể, nên chặn ngay lúc khởi động thay vì để
            // máy chủ chạy với API quản trị phơi trần.
            if (!isLoopback && !hasKey)
            {
                throw new InvalidOperationException(
                    $"HTTP API đặt ở '{BindAddress}' (ngoài loopback) nhưng apiKey trong config/server.json còn rỗng. " +
                    "API này có thể tắt máy chủ và tạo vật phẩm. " +
                    "Đặt apiKey, hoặc đổi httpBindAddress về 127.0.0.1.");
            }

            if (!hasKey)
            {
                GopetManager.ServerMonitor.LogWarning(
                    "HTTP API chạy KHÔNG xác thực (apiKey rỗng). Chỉ chấp nhận được vì đang bind loopback. " +
                    "Đặt apiKey trong config/server.json trước khi mở ra ngoài.");
            }

            Application.RunAsync($"http://{BindAddress}:{this.Port}");
        }

        public void Stop()
        {
            Application.StopAsync();
        }
    }
}
