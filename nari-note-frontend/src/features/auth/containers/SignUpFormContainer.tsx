'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { SignUpForm } from '../components/SignUpForm';
import { useSignUp } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';

/**
 * サインアップフォームコンテナ - Container Component
 * 
 * サインアップのロジックを管理し、SignUpFormに渡します。
 */
export function SignUpFormContainer() {
  const router = useRouter();
  const { login } = useAuth();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirm, setPasswordConfirm] = useState('');
  const [error, setError] = useState<string>();
  
  const signUpMutation = useSignUp({
    onSuccess: (data) => {
      // 登録成功時、AuthProvider経由でログイン状態を更新してホームへリダイレクト
      if (data.userId) {
        login(data.userId);
      }
      router.push('/');
    },
    onError: (err) => {
      // APIエラーメッセージを表示
      if (err instanceof Error) {
        setError(err.message || '新規登録に失敗しました');
      } else {
        setError('新規登録に失敗しました');
      }
    },
  });

  const handleSubmit = async () => {
    setError(undefined);
    
    // バリデーション
    if (!name.trim()) {
      setError('ユーザー名を入力してください');
      return;
    }
    
    if (!email.trim()) {
      setError('メールアドレスを入力してください');
      return;
    }
    
    if (password !== passwordConfirm) {
      setError('パスワードが一致しません');
      return;
    }
    
    if (password.length < 8) {
      setError('パスワードは8文字以上で入力してください');
      return;
    }
    
    // API呼び出し
    signUpMutation.mutate({
      name: name.trim(),
      email: email.trim(),
      password,
    });
  };

  return (
    <SignUpForm
      name={name}
      email={email}
      password={password}
      passwordConfirm={passwordConfirm}
      onNameChange={setName}
      onEmailChange={setEmail}
      onPasswordChange={setPassword}
      onPasswordConfirmChange={setPasswordConfirm}
      onSubmit={handleSubmit}
      isSubmitting={signUpMutation.isPending}
      error={error}
    />
  );
}
