'use client';

import { createContext, useContext, ReactNode } from 'react';
import { useGetCurrentUser } from '@/lib/api';

interface AuthContextType {
  userId: number | null;
  userName: string | null;
  isLoggedIn: boolean;
  isLoading: boolean;
  refetch: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const { data, isLoading, refetch } = useGetCurrentUser({}, {
    retry: false,
    refetchOnWindowFocus: false,
  });

  const userId = data?.userId ?? null;
  const userName = data?.userName ?? null;

  return (
    <AuthContext.Provider
      value={{
        userId,
        userName,
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
