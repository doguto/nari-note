'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { LoginForm } from '../components/LoginForm';
import { useSignIn } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';

export function LoginFormContainer() {
  const router = useRouter();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string>();
  
  const signInMutation = useSignIn({
    onSuccess: (data) => {
      // ログイン成功時、AuthProvider経由でログイン状態を更新してホームへリダイレクト
      if (data.userId) {
        login(data.userId);
      }
      router.push('/');
    },
    onError: (err) => {
      // APIエラーメッセージを表示
      if (err instanceof Error) {
        setError(err.message || 'ログインに失敗しました');
      } else {
        setError('ログインに失敗しました');
      }
    },
  });

  const handleSubmit = async () => {
    setError(undefined);
    
    // バリデーション
    if (!email.trim()) {
      setError('メールアドレスまたはユーザー名を入力してください');
      return;
    }
    
    if (!password) {
      setError('パスワードを入力してください');
      return;
    }
    
    // API呼び出し
    signInMutation.mutate({
      usernameOrEmail: email.trim(),
      password,
    });
  };

  return (
    <LoginForm
      email={email}
      password={password}
      onEmailChange={setEmail}
      onPasswordChange={setPassword}
      onSubmit={handleSubmit}
      isSubmitting={signInMutation.isPending}
      error={error}
    />
  );
}
