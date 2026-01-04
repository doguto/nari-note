import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'http://api:5243/api/:path*',
      },
    ];
  },
};

export default nextConfig;
