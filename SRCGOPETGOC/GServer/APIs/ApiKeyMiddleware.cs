using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace Gopet.APIs
{
    /// <summary>
    /// Bắt buộc có API key cho mọi request tới HTTP API quản trị.
    ///
    /// <para>Không có lớp này thì <c>ServerController</c> phơi ra
    /// <c>/api/server/shutdown</c>, <c>/api/BuffItem/...</c> (tạo vật phẩm bất kỳ
    /// cho nhân vật bất kỳ) và <c>/api/SendMail/...</c> cho bất cứ ai gọi được —
    /// không cần xác thực gì.</para>
    ///
    /// <para>Client gửi key qua header <c>X-Api-Key</c>, hoặc query
    /// <c>?apiKey=...</c> cho tiện khi thử bằng trình duyệt.</para>
    /// </summary>
    public sealed class ApiKeyMiddleware
    {
        public const string HeaderName = "X-Api-Key";
        public const string QueryName = "apiKey";

        private readonly RequestDelegate _next;
        private readonly byte[] _expected;

        public ApiKeyMiddleware(RequestDelegate next, string apiKey)
        {
            _next = next;
            _expected = Encoding.UTF8.GetBytes(apiKey);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsAuthorized(context))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(
                $"{{\"status\":0,\"data\":\"Thiếu hoặc sai API key. Gửi qua header '{HeaderName}' hoặc query '?{QueryName}='.\"}}");
        }

        private bool IsAuthorized(HttpContext context)
        {
            string? provided = context.Request.Headers[HeaderName].FirstOrDefault();

            if (string.IsNullOrEmpty(provided))
            {
                provided = context.Request.Query[QueryName].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(provided))
            {
                return false;
            }

            // So sánh thời gian hằng định: so sánh chuỗi thông thường thoát sớm ở
            // byte đầu tiên khác nhau, để lộ độ dài tiền tố đúng qua thời gian đáp ứng.
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(provided), _expected);
        }
    }

    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseGopetApiKey(this IApplicationBuilder app, string apiKey)
        {
            return app.UseMiddleware<ApiKeyMiddleware>(apiKey);
        }
    }
}
