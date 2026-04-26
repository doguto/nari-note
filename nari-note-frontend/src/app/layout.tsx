
import type { Metadata } from "next";
import "./globals.css";
import { Suspense } from "react";
import { QueryProvider } from "@/lib/providers/QueryProvider";
import { AuthProvider } from "@/lib/providers/AuthProvider";
import { UnauthorizedProvider } from "@/lib/providers/UnauthorizedProvider";
import { GoogleAnalytics } from "@next/third-parties/google";

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || "https://nari-note.com";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: "将棋ブログ投稿サイト ～なりノート～",
    template: "%s | なりノート",
  },
  description: "将棋の記事投稿プラットフォーム。みんなの記事を読んで、将棋の知識を深めよう！",
  openGraph: {
    type: "website",
    locale: "ja_JP",
    url: siteUrl,
    siteName: "なりノート",
    title: "将棋ブログ投稿サイト ～なりノート～",
    description: "将棋の記事投稿プラットフォーム。みんなの記事を読んで、将棋の知識を深めよう！",
    images: [{ url: "/narinote_background.png" }],
  },
  twitter: {
    card: "summary_large_image",
    title: "将棋ブログ投稿サイト ～なりノート～",
    description: "将棋の記事投稿プラットフォーム。みんなの記事を読んで、将棋の知識を深めよう！",
    images: ["/narinote_background.png"],
  },
  robots: {
    index: true,
    follow: true,
  },
  icons: {
    icon: "/favicon.ico",
  }
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
      {/* 漏洩しても問題ないのでそのまま平文 */}
      <GoogleAnalytics gaId="G-293Z66RW8T" />
    </html>
  );
}
