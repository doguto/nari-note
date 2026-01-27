import { Header } from './Header';
import { Footer } from './Footer';

/**
 * メインレイアウトコンポーネント
 * 
 * Header、Footer、および共通のコンテナスタイルを提供します。
 * 全ページで共通のレイアウト構造をDRY原則に従って実装。
 */
export function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gradient-to-b from-brand-bg-gradient-from to-brand-bg-gradient-to flex flex-col">
      <Header />
      <div className="flex-1">
        {children}
      </div>
      <Footer />
    </div>
  );
}
