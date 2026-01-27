import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/ui';
import { EmailField, PasswordField } from '@/components/molecules';

interface LoginTemplateProps {
  email: string;
  password: string;
  error?: string;
  isLoading: boolean;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}

/**
 * LoginTemplate - Template Component
 * 
 * ログインページのUI構成に責任を持つ
 * レスポンシブ対応とレイアウトを担当
 */
export function LoginTemplate({
  email,
  password,
  error,
  isLoading,
  onEmailChange,
  onPasswordChange,
  onSubmit,
}: LoginTemplateProps) {
  return (
    <div className="max-w-2xl w-full mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>ログイン</FormTitle>
      
      {error && <ErrorAlert message={error} />}
      
      <form onSubmit={onSubmit} className="space-y-6">
        <EmailField value={email} onChange={onEmailChange} />
        
        <PasswordField value={password} onChange={onPasswordChange} />
        
        <Button
          type="submit"
          disabled={isLoading}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {isLoading ? 'ログイン中...' : 'ログイン'}
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
