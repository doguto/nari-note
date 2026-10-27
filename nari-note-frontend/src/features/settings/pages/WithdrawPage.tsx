'use client';

import { useState } from 'react';
import { useWithdraw } from '@/lib/api';
import { WithdrawTemplate } from '../templates/WithdrawTemplate';


export function WithdrawPage() {
  const [password, setPassword] = useState('');
  const [passwordError, setPasswordError] = useState<string | undefined>();
  const [generalError, setGeneralError] = useState<string | undefined>();
  const [isConfirmDialogOpen, setIsConfirmDialogOpen] = useState(false);

  const withdraw = useWithdraw({
    onSuccess: () => {
      // 認証状態の再取得で AuthGuard がログイン画面へ遷移しないよう、フルリロードでトップへ戻す
      window.location.replace('/');
    },
    onError: (err) => {
      setIsConfirmDialogOpen(false);
      setGeneralError(err instanceof Error ? err.message : '退会処理に失敗しました');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setGeneralError(undefined);

    if (!password) {
      setPasswordError('パスワードを入力してください');
      return;
    }
    setPasswordError(undefined);
    setIsConfirmDialogOpen(true);
  };

  const handleConfirm = () => {
    withdraw.mutate({ password });
  };

  return (
    <WithdrawTemplate
      password={password}
      passwordError={passwordError}
      generalError={generalError}
      isConfirmDialogOpen={isConfirmDialogOpen}
      isSubmitting={withdraw.isPending}
      onPasswordChange={setPassword}
      onSubmit={handleSubmit}
      onConfirmDialogOpenChange={setIsConfirmDialogOpen}
      onConfirm={handleConfirm}
    />
  );
}
