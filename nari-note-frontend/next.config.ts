import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  transpilePackages: ['react-markdown', 'remark-gfm'],
  async rewrites() {
    // API_URL はビルド時に評価される（Cloudflare Pages のビルド設定で要設定）
    const apiUrl = process.env.API_URL || 'http://localhost:5243';
    return [
      {
        source: '/api/:path*',
        destination: `${apiUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
