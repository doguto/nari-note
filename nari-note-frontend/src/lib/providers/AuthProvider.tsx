'use client';

import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useGetCurrentUser } from '@/lib/api';

interface AuthContextType {
  userId: string | null;
  userName: string | null;
  userIconImageUrl: string | null;
  isLoggedIn: boolean;
  isEmailVerified: boolean;
  isLoading: boolean;
  refetch: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const { data, isLoading, refetch } = useGetCurrentUser({
    retry: false,
    // 別タブでメール認証を完了して戻ってきたときに、リロード無しで認証状態を反映する
    refetchOnWindowFocus: (query) => {
      const current = query.state.data;
      return current?.userId != null && !current.isEmailVerified ? 'always' : false;
    },
  });

  const userId = data?.userId ?? null;
  const isEmailVerified = data?.isEmailVerified ?? false;
  const userName = data?.userName ?? null;
  const rawIconUrl = data?.userIconImageUrl ?? null;
  const [loadedIconUrl, setLoadedIconUrl] = useState<string | null>(null);

  // 読み込みに成功した画像URLのみ公開する
  useEffect(() => {
    if (!rawIconUrl) return;
    const img = new Image();
    img.onload = () => setLoadedIconUrl(rawIconUrl);
    img.src = rawIconUrl;
  }, [rawIconUrl]);

  const userIconImageUrl = rawIconUrl !== null && loadedIconUrl === rawIconUrl ? rawIconUrl : null;

  return (
    <AuthContext.Provider
      value={{
        userId,
        userName,
        userIconImageUrl,
        isLoggedIn: userId !== null,
        isEmailVerified,
        isLoading,
        refetch,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
