'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';


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
