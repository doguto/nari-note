'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/common/atoms';
import { EmailField, PasswordField, NameField } from '@/components/common/molecules';
import { useSignUp } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';

/**
 * SignUpPage - Organism Component
 * 
 * サインアップページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function SignUpPage() {
  const router = useRouter();
  const { login, refetch } = useAuth();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirm, setPasswordConfirm] = useState('');
  const [error, setError] = useState<string>();
  
  const signUpMutation = useSignUp({
    onSuccess: (data) => {
      if (data.userId) {
        login(data.userId);
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
    <div className="max-w-2xl w-full mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>新規登録</FormTitle>
      
      {error && <ErrorAlert message={error} />}
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <NameField value={name} onChange={setName} />
        
        <EmailField value={email} onChange={setEmail} />
        
        <PasswordField 
          value={password} 
          onChange={setPassword}
          helperText="8文字以上で入力してください"
        />
        
        <PasswordField
          value={passwordConfirm}
          onChange={setPasswordConfirm}
          id="passwordConfirm"
          label="パスワード（確認）"
        />
        
        <Button
          type="submit"
          disabled={signUpMutation.isPending}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {signUpMutation.isPending ? '登録中...' : '新規登録'}
        </Button>
      </form>
      
      <div className="mt-8 text-center">
        <p className="text-sm text-gray-600">
          すでにアカウントをお持ちの方は
          <Link href="/login" className="text-brand-primary hover:underline ml-1">
            ログイン
          </Link>
        </p>
      </div>
    </div>
  );
}
