'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';

export default function SignUpPage() {
  const router = useRouter();
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirm, setPasswordConfirm] = useState('');
  const [errors, setErrors] = useState<{
    username?: string;
    email?: string;
    password?: string;
    passwordConfirm?: string;
  }>({});

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const newErrors: typeof errors = {};

    // バリデーション
    if (!username) {
      newErrors.username = 'ユーザー名が必須です';
    } else if (username.length < 3) {
      newErrors.username = 'ユーザー名は3文字以上である必要があります';
    } else if (!/^[a-zA-Z0-9_]+$/.test(username)) {
      newErrors.username = 'ユーザー名は英数字とアンダースコアのみ使用可能です';
    }

    if (!email) {
      newErrors.email = 'メールアドレスが必須です';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'メールアドレスの形式が正しくありません';
    }

    if (!password) {
      newErrors.password = 'パスワードが必須です';
    } else if (password.length < 8) {
      newErrors.password = 'パスワードは8文字以上である必要があります';
    }

    if (password !== passwordConfirm) {
      newErrors.passwordConfirm = 'パスワードが一致しません';
    }

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    // モック：サインアップ成功
    console.log('サインアップ:', { username, email, password });
    router.push('/');
  };

  const handleGoogleSignUp = () => {
    // モック：Google認証
    console.log('Googleでサインアップ');
    router.push('/');
  };

  return (
    <div className="container mx-auto px-4 py-16 flex justify-center">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-2xl">新規登録</CardTitle>
          <CardDescription>アカウントを作成してください</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="username" className="block text-sm font-medium mb-2">
                ユーザー名
              </label>
              <Input
                id="username"
                type="text"
                placeholder="user_name"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className={errors.username ? 'border-red-500' : ''}
              />
              {errors.username && (
                <p className="text-sm text-red-600 mt-1">{errors.username}</p>
              )}
            </div>

            <div>
              <label htmlFor="email" className="block text-sm font-medium mb-2">
                メールアドレス
              </label>
              <Input
                id="email"
                type="email"
                placeholder="email@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={errors.email ? 'border-red-500' : ''}
              />
              {errors.email && (
                <p className="text-sm text-red-600 mt-1">{errors.email}</p>
              )}
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium mb-2">
                パスワード
              </label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={errors.password ? 'border-red-500' : ''}
              />
              {errors.password && (
                <p className="text-sm text-red-600 mt-1">{errors.password}</p>
              )}
            </div>

            <div>
              <label htmlFor="passwordConfirm" className="block text-sm font-medium mb-2">
                パスワード（確認）
              </label>
              <Input
                id="passwordConfirm"
                type="password"
                placeholder="••••••••"
                value={passwordConfirm}
                onChange={(e) => setPasswordConfirm(e.target.value)}
                className={errors.passwordConfirm ? 'border-red-500' : ''}
              />
              {errors.passwordConfirm && (
                <p className="text-sm text-red-600 mt-1">{errors.passwordConfirm}</p>
              )}
            </div>

            <Button type="submit" className="w-full">
              新規登録
            </Button>
          </form>

          <div className="my-6">
            <Separator />
            <div className="relative flex justify-center text-xs uppercase -mt-3">
              <span className="bg-white px-2 text-muted-foreground">または</span>
            </div>
          </div>

          <Button
            type="button"
            variant="outline"
            className="w-full"
            onClick={handleGoogleSignUp}
          >
            <svg className="w-5 h-5 mr-2" viewBox="0 0 24 24">
              <path
                fill="currentColor"
                d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
              />
              <path
                fill="currentColor"
                d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
              />
              <path
                fill="currentColor"
                d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
              />
              <path
                fill="currentColor"
                d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
              />
            </svg>
            Googleでサインアップ
          </Button>

          <div className="mt-6 text-center text-sm">
            <span className="text-gray-600">すでにアカウントをお持ちですか？ </span>
            <Link href="/signin" className="text-blue-600 hover:underline">
              サインイン
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
