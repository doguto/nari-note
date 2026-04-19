'use client';

import { useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useResetPassword } from '@/lib/api';
import { AuthPageLayout } from '@/components/molecules';
import { ResetPasswordTemplate } from '../templates/ResetPasswordTemplate';

export function ResetPasswordPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token');

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string>();
  const [isSuccess, setIsSuccess] = useState(false);

  const resetPasswordMutation = useResetPassword({
    onSuccess: () => {
      setIsSuccess(true);
    },
    onError: (err) => {
      if (err instanceof Error) {
        setError(err.message || 'パスワードの更新に失敗しました');
      } else {
        setError('パスワードの更新に失敗しました');
      }
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(undefined);

    if (!password) {
      setError('新しいパスワードを入力してください');
      return;
    }

    if (password.length < 8) {
      setError('パスワードは8文字以上で入力してください');
      return;
    }

    if (password !== confirmPassword) {
      setError('パスワードが一致しません');
      return;
    }

    resetPasswordMutation.mutate({ token: token!, newPassword: password });
  };

  return (
    <AuthPageLayout>
      <ResetPasswordTemplate
        password={password}
        confirmPassword={confirmPassword}
        error={error}
        isLoading={resetPasswordMutation.isPending}
        isSuccess={isSuccess}
        isTokenMissing={!token}
        onPasswordChange={setPassword}
        onConfirmPasswordChange={setConfirmPassword}
        onSubmit={handleSubmit}
      />
    </AuthPageLayout>
  );
}
