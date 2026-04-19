'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useLogout } from '@/lib/api';
import { HeaderLogo } from './HeaderLogo';
import { HeaderNav } from './HeaderNav';
import { HeaderNavMobile } from './HeaderNavMobile';
import { HeaderUserMenu } from './HeaderUserMenu';
import { HeaderAuthButtons } from './HeaderAuthButtons';


export function Header() {
  const [isMounted, setIsMounted] = useState(false);
  const { userId, userName, userIconImageUrl, isLoggedIn, isLoading, refetch } = useAuth();
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
      <HeaderLogo />

      <div className="bg-brand-text border-b border-brand-text-dark shadow-sm">
        <div className="w-11/12 mx-auto px-4 py-2.5 flex items-center justify-end md:justify-center relative">
          <HeaderNav />

          <div className="flex items-center gap-4 md:absolute md:right-4">
            {!isMounted || isLoading ? (
              <div className="w-24 h-6 bg-brand-text-dark rounded animate-pulse" />
            ) : isLoggedIn ? (
              <HeaderUserMenu
                userId={userId!}
                userName={userName!}
                userIconImageUrl={userIconImageUrl ?? undefined}
                onLogout={handleLogout}
                isLoggingOut={logoutMutation.isPending}
              />
            ) : (
              <HeaderAuthButtons />
            )}
          </div>
        </div>

        <HeaderNavMobile />
      </div>
    </header>
  );
}
