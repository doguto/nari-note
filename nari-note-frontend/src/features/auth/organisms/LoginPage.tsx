'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/common/atoms';
import { EmailField, PasswordField } from '@/components/common/molecules';
import { useSignIn } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';

/**
 * LoginPage - Organism Component
 * 
 * ログインページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string>();
  
  const signInMutation = useSignIn({
    onSuccess: (data) => {
      if (data.userId) {
        login(data.userId);
      }
      router.push('/');
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
    <div className="max-w-2xl w-full mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>ログイン</FormTitle>
      
      {error && <ErrorAlert message={error} />}
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <EmailField value={email} onChange={setEmail} />
        
        <PasswordField value={password} onChange={setPassword} />
        
        <Button
          type="submit"
          disabled={signInMutation.isPending}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {signInMutation.isPending ? 'ログイン中...' : 'ログイン'}
        </Button>
      </form>
      
      <div className="mt-8 text-center">
        <p className="text-sm text-gray-600">
          アカウントをお持ちでない方は
          <Link href="/signup" className="text-brand-primary hover:underline ml-1">
            新規登録
          </Link>
        </p>
      </div>
    </div>
  );
}
