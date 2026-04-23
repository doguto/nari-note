'use client';

import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useGetCurrentUser } from '@/lib/api';

interface AuthContextType {
  userId: string | null;
  userName: string | null;
  userIconImageUrl: string | null;
  isLoggedIn: boolean;
  isLoading: boolean;
  refetch: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const { data, isLoading, refetch } = useGetCurrentUser({
    retry: false,
    refetchOnWindowFocus: false,
  });

  const userId = data?.userId ?? null;
  const userName = data?.userName ?? null;
  const [userIconImageUrl, setUserIconImageUrl] = useState<string | null>(null);

  useEffect(() => {
    const rawUrl = data?.userIconImageUrl ?? null;
    if (!rawUrl) {
      setUserIconImageUrl(null);
      return;
    }
    const img = new Image();
    img.onload = () => setUserIconImageUrl(rawUrl);
    img.onerror = () => setUserIconImageUrl(null);
    img.src = rawUrl;
  }, [data?.userIconImageUrl]);

  return (
    <AuthContext.Provider
      value={{
        userId,
        userName,
        userIconImageUrl,
        isLoggedIn: userId !== null,
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
