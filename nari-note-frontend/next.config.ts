import type { NextConfig } from "next";

// Cloudflare Pages ローカル開発時のプラットフォームエミュレーション
if (process.env.NODE_ENV === 'development') {
  void import('@cloudflare/next-on-pages/next-dev').then(({ setupDevPlatform }) => setupDevPlatform());
}

const nextConfig: NextConfig = {
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
