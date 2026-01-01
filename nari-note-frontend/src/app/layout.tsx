import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "なりノート",
  description: "持続の裁判、ソノビビュー、自我記。知識分全全を譲しよ。う",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ja">
      <body className="antialiased">
        {children}
      </body>
    </html>
  );
}
