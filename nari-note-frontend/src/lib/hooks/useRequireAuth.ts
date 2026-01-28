'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';

/**
 * 認証が必要なページで使用するカスタムフック
 * 未認証の場合、ログインページへリダイレクトする
 * 
 * @param redirectPath - ログイン後にリダイレクトする元のパス（オプション）
 * @returns 認証状態オブジェクト { isLoggedIn, isLoading, userId }
 */
export function useRequireAuth(redirectPath?: string) {
  const router = useRouter();
  const auth = useAuth();
  const { isLoggedIn, isLoading } = auth;

  useEffect(() => {
    if (!isLoading && !isLoggedIn) {
      const loginUrl = redirectPath 
        ? `/login?redirect=${encodeURIComponent(redirectPath)}`
        : '/login';
      router.push(loginUrl);
    }
  }, [isLoggedIn, isLoading, router, redirectPath]);

  return auth;
}
