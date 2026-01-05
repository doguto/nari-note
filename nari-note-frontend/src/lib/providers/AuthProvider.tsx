'use client';

import { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';

interface AuthContextType {
  userId: number | null;
  isLoggedIn: boolean;
  isLoading: boolean;
  login: (userId: number) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [userId, setUserId] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // 初期化時にlocalStorageからユーザーIDを取得
    const storedUserId = localStorage.getItem('userId');
    if (storedUserId) {
      setUserId(parseInt(storedUserId, 10));
    }
    setIsLoading(false);
  }, []);

  const login = useCallback((newUserId: number) => {
    localStorage.setItem('userId', newUserId.toString());
    setUserId(newUserId);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('userId');
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
