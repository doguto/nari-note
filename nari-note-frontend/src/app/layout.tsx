
import type { Metadata } from "next";
import { SITE_DESCRIPTION, SITE_NAME, SITE_TITLE } from "@/lib/metadata";
import "./globals.css";
import { Suspense } from "react";
import { QueryProvider } from "@/lib/providers/QueryProvider";
import { AuthProvider } from "@/lib/providers/AuthProvider";
import { UnauthorizedProvider } from "@/lib/providers/UnauthorizedProvider";
import { ForbiddenProvider } from "@/lib/providers/ForbiddenProvider";
import { GoogleAnalytics } from "@next/third-parties/google";

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || "https://nari-note.com";

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
    <html lang="ja">
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
