import { ReactNode } from 'react';

interface AuthPageLayoutProps {
  children: ReactNode;
}

/**
 * AuthPageLayout - Molecule Component
 * 
 * 認証ページ（ログイン・サインアップ）のレイアウトを提供するコンポーネント
 * 中央配置と適切なパディングを適用
 */
export function AuthPageLayout({ children }: AuthPageLayoutProps) {
  return (
    <div className="flex items-center justify-center py-12 px-4">
      {children}
    </div>
  );
}
