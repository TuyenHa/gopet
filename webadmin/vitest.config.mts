import path from "node:path";
import { loadEnv } from "vite";
import { defineConfig } from "vitest/config";

export default defineConfig(({ mode }) => ({
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "src"),
      // "server-only" ném lỗi ngoài môi trường React Server — test chạy trên Node thuần.
      "server-only": path.resolve(import.meta.dirname, "tests/server-only-stub.ts"),
    },
  },
  test: {
    environment: "node",
    include: ["tests/**/*.test.ts"],
    // Nạp .env/.env.local (không bắt buộc tồn tại) — integration test cần DB_*; chỉ chạy khi RUN_DB_TESTS=1.
    env: loadEnv(mode, import.meta.dirname, ""),
  },
}));
