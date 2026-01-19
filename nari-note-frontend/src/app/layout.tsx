import type { Metadata } from "next";
import "./globals.css";
import { QueryProvider } from "@/lib/providers/QueryProvider";
import { AuthProvider } from "@/lib/providers/AuthProvider";
import { UnauthorizedProvider } from "@/lib/providers/UnauthorizedProvider";

export const metadata: Metadata = {
  title: "将棋ブログ投稿サイト ～なりノート～",
  description: "将棋の知識共有プラットフォーム\nあなたの将棋の知識を共有し、コミュニティと共に成長しましょう",
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
            <UnauthorizedProvider>
              {children}
            </UnauthorizedProvider>
          </AuthProvider>
        </QueryProvider>
      </body>
    </html>
  );
}
