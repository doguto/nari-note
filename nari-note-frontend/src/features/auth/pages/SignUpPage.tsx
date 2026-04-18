'use client';

import { useState } from 'react';
import { useSignUp } from '@/lib/api';
import { AuthPageLayout } from '@/components/molecules';
import { SignUpTemplate } from '../templates/SignUpTemplate';


export function SignUpPage() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirm, setPasswordConfirm] = useState('');
  const [error, setError] = useState<string>();
  const [isCompleted, setIsCompleted] = useState(false);

  const signUpMutation = useSignUp({
    onSuccess: () => {
      setIsCompleted(true);
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

    if (password.length < 8) {
      setError('パスワードは8文字以上で入力してください');
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
    <AuthPageLayout>
      <SignUpTemplate
        name={name}
        email={email}
        password={password}
        passwordConfirm={passwordConfirm}
        error={error}
        isLoading={signUpMutation.isPending}
        isCompleted={isCompleted}
        onNameChange={setName}
        onEmailChange={setEmail}
        onPasswordChange={setPassword}
        onPasswordConfirmChange={setPasswordConfirm}
        onSubmit={handleSubmit}
      />
    </AuthPageLayout>
  );
}
