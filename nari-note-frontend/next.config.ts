import type { NextConfig } from "next";

const imageHostname = process.env.NEXT_PUBLIC_IMAGE_HOSTNAME ?? "image.nari-note.com";

const nextConfig: NextConfig = {
  transpilePackages: ['react-markdown', 'remark-gfm'],
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: imageHostname,
      },
    ],
  },
};

export default nextConfig;
