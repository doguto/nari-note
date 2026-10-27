'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { useVerifyEmail } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { AuthPageLayout } from '@/components/molecules';
import { VerifyEmailTemplate } from '../templates/VerifyEmailTemplate';

export function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const token = searchParams.get('token');
  const { refetch } = useAuth();
  const [verifyStatus, setVerifyStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [verifyError, setVerifyError] = useState<string>();

  const verifyEmailMutation = useVerifyEmail({
    onSuccess: () => {
      refetch();
      setVerifyStatus('success');
    },
    onError: (err) => {
      setVerifyError(err instanceof Error ? err.message : '認証に失敗しました');
      setVerifyStatus('error');
    },
  });

  useEffect(() => {
    if (!token) return;
    verifyEmailMutation.mutate({ token });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const status = token ? verifyStatus : 'error';
  const error = token ? verifyError : 'トークンが見つかりません';

  return (
    <AuthPageLayout>
      <VerifyEmailTemplate status={status} error={error} />
    </AuthPageLayout>
  );
}
