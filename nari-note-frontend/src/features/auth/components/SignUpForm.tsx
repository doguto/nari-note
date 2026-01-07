import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface SignUpFormProps {
  name: string;
  email: string;
  password: string;
  passwordConfirm: string;
  onNameChange: (value: string) => void;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onPasswordConfirmChange: (value: string) => void;
  onSubmit: () => void;
  isSubmitting: boolean;
  error?: string;
}

export function SignUpForm({
  name,
  email,
  password,
  passwordConfirm,
  onNameChange,
  onEmailChange,
  onPasswordChange,
  onPasswordConfirmChange,
  onSubmit,
  isSubmitting,
  error,
}: SignUpFormProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit();
  };

  return (
    <div className="max-w-md mx-auto bg-white rounded-lg shadow-lg p-8">
      <h1 className="text-3xl font-bold text-brand-text mb-6 text-center" style={{ fontFamily: 'serif' }}>
        新規登録
      </h1>
      
      {error && (
        <div className="bg-red-50 border border-red-200 rounded p-3 mb-4">
          <p className="text-red-600 text-sm">{error}</p>
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-2">
          <Label htmlFor="name">
            ユーザー名
          </Label>
          <Input
            id="name"
            type="text"
            value={name}
            onChange={(e) => onNameChange(e.target.value)}
            placeholder="山田太郎"
            required
          />
        </div>
        
        <div className="space-y-2">
          <Label htmlFor="email">
            メールアドレス
          </Label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e) => onEmailChange(e.target.value)}
            placeholder="example@email.com"
            required
          />
        </div>
        
        <div className="space-y-2">
          <Label htmlFor="password">
            パスワード
          </Label>
          <Input
            id="password"
            type="password"
            value={password}
            onChange={(e) => onPasswordChange(e.target.value)}
            placeholder="••••••••"
            required
            minLength={8}
          />
          <p className="text-xs text-gray-500">
            8文字以上で入力してください
          </p>
        </div>
        
        <div className="space-y-2">
          <Label htmlFor="passwordConfirm">
            パスワード（確認）
          </Label>
          <Input
            id="passwordConfirm"
            type="password"
            value={passwordConfirm}
            onChange={(e) => onPasswordConfirmChange(e.target.value)}
            placeholder="••••••••"
            required
            minLength={8}
          />
        </div>
        
        <Button
          type="submit"
          disabled={isSubmitting}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {isSubmitting ? '登録中...' : '新規登録'}
        </Button>
      </form>
      
      <div className="mt-6 text-center">
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
