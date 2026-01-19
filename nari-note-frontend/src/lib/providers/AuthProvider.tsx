'use client';

import { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import { useMe } from '@/lib/api';

interface AuthContextType {
  userId: number | null;
  isLoggedIn: boolean;
  isLoading: boolean;
  login: (userId: number) => void;
  logout: () => void;
  refetch: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [userId, setUserId] = useState<number | null>(null);
  const { data, isLoading, refetch } = useMe({
    retry: false,
    refetchOnWindowFocus: false,
  });

  useEffect(() => {
    if (data?.userId) {
      setUserId(data.userId);
    } else if (!isLoading && data !== undefined) {
      // データがロード済みでuserIdがnullの場合は、未ログイン状態
      setUserId(null);
    }
  }, [data, isLoading]);

  const login = useCallback((newUserId: number) => {
    setUserId(newUserId);
  }, []);

  const logout = useCallback(() => {
    setUserId(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        userId,
        isLoggedIn: userId !== null,
        isLoading,
        login,
        logout,
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
