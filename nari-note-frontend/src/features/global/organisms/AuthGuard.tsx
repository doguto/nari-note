'use client';

import { ReactNode, useState, useEffect } from 'react';
import { useRequireAuth } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';

interface AuthGuardProps {
  children: ReactNode;
  redirectPath: string;
}


export function AuthGuard({ children, redirectPath }: AuthGuardProps) {
  const { isLoggedIn, isLoading } = useRequireAuth(redirectPath);
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  // サーバーサイドとクライアントサイドの初期レンダリングで同じコンテンツを返す
  if (!isMounted || isLoading) {
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
