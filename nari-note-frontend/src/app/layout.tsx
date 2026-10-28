
import type { Metadata } from "next";
import { Shippori_Mincho } from "next/font/google";
import { SITE_DESCRIPTION, SITE_NAME, SITE_TITLE } from "@/lib/metadata";
import "./globals.css";
import { Suspense } from "react";
import { QueryProvider } from "@/lib/providers/QueryProvider";
import { AuthProvider } from "@/lib/providers/AuthProvider";
import { UnauthorizedProvider } from "@/lib/providers/UnauthorizedProvider";
import { ForbiddenProvider } from "@/lib/providers/ForbiddenProvider";
import { GoogleAnalytics } from "@next/third-parties/google";

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || "https://nari-note.com";

// 駒の文字用
const shipporiMincho = Shippori_Mincho({
  weight: "700",
  subsets: ["latin"],
  display: "swap",
  preload: false,
  variable: "--font-shippori-mincho",
});

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: SITE_TITLE,
    template: "%s | なりノート",
  },
  description: SITE_DESCRIPTION,
  openGraph: {
    type: "website",
    locale: "ja_JP",
    url: siteUrl,
    siteName: SITE_NAME,
    title: SITE_TITLE,
    description: SITE_DESCRIPTION
  },
  twitter: {
    card: "summary_large_image",
    title: SITE_TITLE,
    description: SITE_DESCRIPTION
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
    <html lang="ja" className={shipporiMincho.variable}>
      <body className="antialiased">
        <QueryProvider>
          <AuthProvider>
            <Suspense fallback={null}>
              <UnauthorizedProvider>
                <ForbiddenProvider>
                  {children}
                </ForbiddenProvider>
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
