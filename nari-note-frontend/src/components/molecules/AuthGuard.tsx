'use client';

import { ReactNode } from 'react';
import { useRequireAuth } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';

interface AuthGuardProps {
  children: ReactNode;
  redirectPath: string;
}

/**
 * AuthGuard - Molecule Component
 * 
 * 認証が必要なページをラップするコンポーネント
 * 認証チェックとローディング状態の表示を統一的に処理する
 * 
 * @param children - 認証後に表示するコンテンツ
 * @param redirectPath - ログイン後にリダイレクトするパス
 */
export function AuthGuard({ children, redirectPath }: AuthGuardProps) {
  const { isLoggedIn, isLoading } = useRequireAuth(redirectPath);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <LoadingSpinner text="読み込み中..." />
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return <>{children}</>;
}
