'use client';

import { ReactNode, useEffect } from 'react';
import { useRequireVerifiedEmail } from '@/lib/hooks';
import { LoadingSpinner } from '@/components/ui';
import { Button } from '@/components/ui/button';

interface VerifiedEmailGuardProps {
  children: ReactNode;
}

export function VerifiedEmailGuard({ children }: VerifiedEmailGuardProps) {
  const { isBlocked, isLoading, ensureVerifiedEmail } = useRequireVerifiedEmail();

  useEffect(() => {
    ensureVerifiedEmail();
  }, [ensureVerifiedEmail]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <LoadingSpinner text="読み込み中..." />
      </div>
    );
  }

  if (isBlocked) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 min-h-[50vh] px-4 text-center">
        <h1 className="text-xl font-bold text-brand-text">メールアドレスの認証が必要です</h1>
        <p className="text-brand-secondary-text">
          このページを利用するには、メールアドレスの認証が必要です。確認メールのリンクから認証を完了してください。
        </p>
        <Button
          onClick={ensureVerifiedEmail}
          className="bg-brand-primary hover:bg-brand-primary-hover"
        >
          確認メールを再送する
        </Button>
      </div>
    );
  }

  return <>{children}</>;
}
