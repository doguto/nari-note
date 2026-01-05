'use client';

import { useState } from 'react';
import { LoginForm } from '../components/LoginForm';

/**
 * ログインフォームコンテナ - Container Component
 * 
 * ログインのロジックを管理し、LoginFormに渡します。
 * TODO: 実際の認証APIと連携
 */
export function LoginFormContainer() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string>();

  const handleSubmit = async () => {
    setError(undefined);
    setIsSubmitting(true);
    
    try {
      // TODO: 認証APIを実装後、ここにログインロジックを追加
      console.log('Login attempt:', { email, password });
      
      // 仮の処理
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // 成功時の処理
      alert('ログイン機能は実装中です');
    } catch (err) {
      setError('ログインに失敗しました');
      console.error('Login error:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <LoginForm
      email={email}
      password={password}
      onEmailChange={setEmail}
      onPasswordChange={setPassword}
      onSubmit={handleSubmit}
      isSubmitting={isSubmitting}
      error={error}
    />
  );
}
