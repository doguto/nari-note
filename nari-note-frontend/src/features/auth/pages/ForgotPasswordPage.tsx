'use client';

import { useState } from 'react';
import { useForgotPassword } from '@/lib/api';
import { AuthPageLayout } from '@/components/molecules';
import { ForgotPasswordTemplate } from '../templates/ForgotPasswordTemplate';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string>();
  const [isSuccess, setIsSuccess] = useState(false);

  const forgotPasswordMutation = useForgotPassword({
    onSuccess: () => {
      setIsSuccess(true);
    },
    onError: (err) => {
      if (err instanceof Error) {
        setError(err.message || 'メールの送信に失敗しました');
      } else {
        setError('メールの送信に失敗しました');
      }
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(undefined);

    if (!email.trim()) {
      setError('メールアドレスを入力してください');
      return;
    }

    forgotPasswordMutation.mutate({ email: email.trim() });
  };

  return (
    <AuthPageLayout>
      <ForgotPasswordTemplate
        email={email}
        error={error}
        isLoading={forgotPasswordMutation.isPending}
        isSuccess={isSuccess}
        onEmailChange={setEmail}
        onSubmit={handleSubmit}
      />
    </AuthPageLayout>
  );
}
