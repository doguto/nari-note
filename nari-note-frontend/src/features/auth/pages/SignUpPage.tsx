'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useSignUp } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { SignUpTemplate } from '../templates/SignUpTemplate';

/**
 * SignUpPage - Page Component
 * 
 * サインアップページのロジックに責任を持つ
 * UIには一切責任を持たない
 * バックエンドとの通信等の非UIロジックを持つ
 */
export function SignUpPage() {
  const router = useRouter();
  const { refetch } = useAuth();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirm, setPasswordConfirm] = useState('');
  const [error, setError] = useState<string>();
  
  const signUpMutation = useSignUp({
    onSuccess: (data) => {
      if (data.userId) {
        refetch();
      }
      router.push('/');
    },
    onError: (err) => {
      if (err instanceof Error) {
        setError(err.message || '登録に失敗しました');
      } else {
        setError('登録に失敗しました');
      }
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(undefined);
    
    if (!name.trim()) {
      setError('ユーザー名を入力してください');
      return;
    }
    
    if (!email.trim()) {
      setError('メールアドレスを入力してください');
      return;
    }
    
    if (!password) {
      setError('パスワードを入力してください');
      return;
    }
    
    if (password !== passwordConfirm) {
      setError('パスワードが一致しません');
      return;
    }
    
    signUpMutation.mutate({
      name: name.trim(),
      email: email.trim(),
      password,
    });
  };

  return (
    <SignUpTemplate
      name={name}
      email={email}
      password={password}
      passwordConfirm={passwordConfirm}
      error={error}
      isLoading={signUpMutation.isPending}
      onNameChange={setName}
      onEmailChange={setEmail}
      onPasswordChange={setPassword}
      onPasswordConfirmChange={setPasswordConfirm}
      onSubmit={handleSubmit}
    />
  );
}
