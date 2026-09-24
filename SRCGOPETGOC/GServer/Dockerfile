# Image cho máy chủ game goPet (GServer). Dựng từ thư mục GServer:
#
#   docker build -t gopet-gserver .
#
# Thường không build tay mà chạy qua docker/docker-compose.yml (profile "server"),
# xem docs/deployment-linux-backend.md.

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore riêng một lớp: đổi code không phải tải lại NuGet.
COPY Gopet.csproj ./
RUN dotnet restore Gopet.csproj -r linux-x64

COPY . .
RUN dotnet publish Gopet.csproj -c Release -r linux-x64 --self-contained false \
        --no-restore -o /app

# ---------- runtime ----------
# aspnet chứ không phải runtime: Gopet.runtimeconfig.json đòi Microsoft.AspNetCore.App
# (HTTP API quản trị chạy chung tiến trình).
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Font Arial cho captcha (Data/User/GopetCaptcha.cs). SixLabors.Fonts tìm đúng TÊN
# "Arial", không dùng alias của fontconfig — Liberation/DejaVu không thay được.
# Gói nằm ở kho "contrib" của Debian và tự tải font khi cài, nên cần chấp nhận EULA trước.
RUN sed -i 's/^Components: main$/Components: main contrib/' /etc/apt/sources.list.d/debian.sources \
    && echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" \
        | debconf-set-selections \
    && apt-get update \
    && apt-get install -y --no-install-recommends ttf-mscorefonts-installer fontconfig tzdata \
    && fc-list | grep -qi arial \
    && rm -rf /var/lib/apt/lists/*

# Server tự bảo trì lúc hourMaintenance (config/server.json) theo GIỜ MÁY.
ENV TZ=Asia/Ho_Chi_Minh \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

# Mọi đường dẫn của server tính từ thư mục làm việc: config/, assets/, log/, backup_sql/.
# Không chạy bằng root. Server ghi file ngay trong /app (log, backup, json dữ liệu) nên
# user phải sở hữu cả thư mục. Tạo user TRƯỚC rồi COPY --chown: chown -R sau khi copy
# sẽ nhân đôi cả lớp assets trong image.
RUN useradd --uid 10001 --home-dir /app --no-create-home gopet \
    && mkdir -p /app/log /app/backup_sql \
    && chown -R gopet:gopet /app
WORKDIR /app
COPY --from=build --chown=gopet:gopet /app ./
USER gopet

EXPOSE 19180

# Chạy thẳng dotnet (PID 1) để nhận SIGTERM từ `docker stop` — App/GracefulShutdown.cs
# bắt tín hiệu đó để lưu dữ liệu trước khi thoát.
ENTRYPOINT ["dotnet", "Gopet.dll"]
