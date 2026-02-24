'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useLogout } from '@/lib/api';
import { HeaderLogo } from './HeaderLogo';
import { HeaderNav } from './HeaderNav';
import { HeaderUserMenu } from './HeaderUserMenu';
import { HeaderAuthButtons } from './HeaderAuthButtons';

/**
 * ヘッダーコンポーネント
 *
 * 全ページ共通のヘッダー。
 * ロゴ、ナビゲーション、ユーザーメニューを表示します。
 */
export function Header() {
  const [isMounted, setIsMounted] = useState(false);
  const { userId, userName, isLoggedIn, isLoading, refetch } = useAuth();
  const logoutMutation = useLogout({
    onSuccess: () => {
      refetch();
    },
  });

  useEffect(() => {
    setIsMounted(true);
  }, []);

  const handleLogout = () => {
    logoutMutation.mutate();
  };

  return (
    <header>
      {/* Top row: Site title with ochre/tan background */}
      <HeaderLogo />

      {/* Bottom row: Navigation menu with dark background */}
      <div className="bg-brand-text border-b border-brand-text-dark shadow-sm">
        <div className="w-11/12 mx-auto px-4 py-2.5 flex items-center justify-center relative">
          <HeaderNav />

          <div className="flex items-center gap-4 absolute right-4">
            {!isMounted || isLoading ? (
              // ローディング中またはマウント前はスケルトン表示
              <div className="w-24 h-6 bg-brand-text-dark rounded animate-pulse" />
            ) : isLoggedIn ? (
              // ログイン時: マイページメニューとログアウトボタン
              <HeaderUserMenu
                userId={userId!}
                userName={userName!}
                onLogout={handleLogout}
                isLoggingOut={logoutMutation.isPending}
              />
            ) : (
              // 未ログイン時: ログイン・新規登録ボタン
              <HeaderAuthButtons />
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
