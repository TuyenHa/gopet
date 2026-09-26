# Image cho trang quản trị Gopet (Next.js 16, App Router). Build trên GitHub Actions,
# push GHCR; máy chủ chỉ `docker compose pull` (xem docs/deployment-linux-backend.md).
#
#   docker build -t gopet-webadmin .        # build thử tay, cần mạng (tải font Google)
#
# 3 tầng: deps (cài package) → build (next build, cần next.config.ts output:"standalone")
# → runtime (chỉ copy .next/standalone + static + public, chạy user non-root).

# ---------- deps ----------
FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
# npm ci cần cả devDependencies (typescript, tailwind...) để build ở tầng sau.
RUN npm ci

# ---------- build ----------
FROM node:22-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
# next/font/google (layout.tsx) tải font lúc build — cần mạng ra ngoài.
RUN npm run build

# ---------- runtime ----------
FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    TZ=Asia/Ho_Chi_Minh \
    PORT=3000 \
    HOSTNAME=0.0.0.0

# output:"standalone" đã gói sẵn node_modules cần thiết vào .next/standalone —
# KHÔNG copy node_modules / package.json ở tầng deps vào đây (image bị to ra vô ích).
COPY --from=build /app/public ./public
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static

# Image alpine của Node đã có sẵn user "node" (uid 1000) — không cần useradd riêng.
USER node
EXPOSE 3000

# Healthcheck thật nằm ở docker-compose.yml (biết tên service); dòng này chỉ để
# `docker inspect` báo đúng trạng thái khi chạy container đứng một mình.
HEALTHCHECK --interval=15s --timeout=5s --retries=5 --start-period=30s \
  CMD wget -qO- http://127.0.0.1:3000/api/health || exit 1

CMD ["node", "server.js"]
