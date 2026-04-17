'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { useVerifyEmail } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { AuthPageLayout } from '@/components/molecules';
import { VerifyEmailTemplate } from '../templates/VerifyEmailTemplate';

export function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const { refetch } = useAuth();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [error, setError] = useState<string>();

  const verifyEmailMutation = useVerifyEmail({
    onSuccess: () => {
      refetch();
      setStatus('success');
    },
    onError: (err) => {
      setError(err instanceof Error ? err.message : '認証に失敗しました');
      setStatus('error');
    },
  });

  useEffect(() => {
    const token = searchParams.get('token');
    if (!token) {
      setError('トークンが見つかりません');
      setStatus('error');
      return;
    }
    verifyEmailMutation.mutate({ token });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <AuthPageLayout>
      <VerifyEmailTemplate status={status} error={error} />
    </AuthPageLayout>
  );
}
