import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Image Docker gọn: chỉ copy .next/standalone + static (phase 11).
  output: "standalone",
  poweredByHeader: false,
};

export default nextConfig;
