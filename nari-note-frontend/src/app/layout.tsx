import type { Metadata } from "next";
import "./globals.css";
import { Suspense } from "react";
import { QueryProvider } from "@/lib/providers/QueryProvider";
import { AuthProvider } from "@/lib/providers/AuthProvider";
import { UnauthorizedProvider } from "@/lib/providers/UnauthorizedProvider";

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || "https://nari-note.com";
const siteName = "なりノート";
const siteTitle = "将棋ブログ投稿サイト ～なりノート～";
const siteDescription =
  "将棋の知識共有プラットフォーム。あなたの将棋の知識を共有し、コミュニティと共に成長しましょう。";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: siteTitle,
    template: `%s | ${siteName}`,
  },
  description: siteDescription,
  openGraph: {
    type: "website",
    locale: "ja_JP",
    siteName,
    title: siteTitle,
    description: siteDescription,
    url: siteUrl,
  },
  twitter: {
    card: "summary_large_image",
    title: siteTitle,
    description: siteDescription,
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ja">
      <body className="antialiased">
        <QueryProvider>
          <AuthProvider>
            <Suspense fallback={null}>
              <UnauthorizedProvider>
                {children}
              </UnauthorizedProvider>
            </Suspense>
          </AuthProvider>
        </QueryProvider>
      </body>
    </html>
  );
}
