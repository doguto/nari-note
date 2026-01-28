'use client';

import { useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useSignIn } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { AuthPageLayout } from '@/components/molecules';
import { LoginTemplate } from '../templates/LoginTemplate';

/**
 * LoginPage - Page Component
 * 
 * ログインページのロジックに責任を持つ
 * UIには一切責任を持たない
 * バックエンドとの通信等の非UIロジックを持つ
 */
export function LoginPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { refetch } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string>();
  
  const signInMutation = useSignIn({
    onSuccess: (data) => {
      if (data.userId) {
        refetch();
      }
      // redirectパラメータがあれば、そのページに遷移
      const redirectPath = searchParams.get('redirect');
      
      // セキュリティ: 相対パスのみ許可（外部URLへのリダイレクトを防ぐ）
      // /で始まり、//で始まらず、バックスラッシュを含まないパスのみ許可
      if (
        redirectPath && 
        redirectPath.startsWith('/') && 
        !redirectPath.startsWith('//') &&
        !redirectPath.includes('\\')
      ) {
        router.push(redirectPath);
      } else {
        router.push('/');
      }
    },
    onError: (err) => {
      if (err instanceof Error) {
        setError(err.message || 'ログインに失敗しました');
      } else {
        setError('ログインに失敗しました');
      }
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(undefined);
    
    if (!email.trim()) {
      setError('メールアドレスまたはユーザー名を入力してください');
      return;
    }
    
    if (!password) {
      setError('パスワードを入力してください');
      return;
    }
    
    signInMutation.mutate({
      usernameOrEmail: email.trim(),
      password,
    });
  };

  return (
    <AuthPageLayout>
      <LoginTemplate
        email={email}
        password={password}
        error={error}
        isLoading={signInMutation.isPending}
        onEmailChange={setEmail}
        onPasswordChange={setPassword}
        onSubmit={handleSubmit}
      />
    </AuthPageLayout>
  );
}
