'use client';

import { useState } from 'react';
import { SignUpForm } from '../components/SignUpForm';

/**
 * サインアップフォームコンテナ - Container Component
 * 
 * サインアップのロジックを管理し、SignUpFormに渡します。
 * TODO: 実際の認証APIと連携
 */
export function SignUpFormContainer() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirm, setPasswordConfirm] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string>();

  const handleSubmit = async () => {
    setError(undefined);
    
    // バリデーション
    if (password !== passwordConfirm) {
      setError('パスワードが一致しません');
      return;
    }
    
    if (password.length < 8) {
      setError('パスワードは8文字以上で入力してください');
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      // TODO: 認証APIを実装後、ここにサインアップロジックを追加
      console.log('SignUp attempt:', { name, email, password });
      
      // 仮の処理
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // 成功時の処理
      alert('新規登録機能は実装中です');
    } catch (err) {
      setError('新規登録に失敗しました');
      console.error('SignUp error:', err);
    } finally {
      setIsSubmitting(false);
    }
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
      isSubmitting={isSubmitting}
      error={error}
    />
  );
}
