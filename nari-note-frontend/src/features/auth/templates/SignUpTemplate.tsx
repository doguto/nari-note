import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/ui';
import { EmailField, PasswordField, NameField } from '@/components/molecules';

interface SignUpTemplateProps {
  name: string;
  email: string;
  password: string;
  passwordConfirm: string;
  error?: string;
  isLoading: boolean;
  isCompleted?: boolean;
  onNameChange: (value: string) => void;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onPasswordConfirmChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}

/**
 * SignUpTemplate - Template Component
 * 
 * サインアップページのUI構成に責任を持つ
 * レスポンシブ対応とレイアウトを担当
 */
export function SignUpTemplate({
  name,
  email,
  password,
  passwordConfirm,
  error,
  isLoading,
  isCompleted,
  onNameChange,
  onEmailChange,
  onPasswordChange,
  onPasswordConfirmChange,
  onSubmit,
}: SignUpTemplateProps) {
  if (isCompleted) {
    return (
      <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12 text-center">
        <FormTitle>登録完了</FormTitle>
        <div className="mt-6 space-y-4">
          <p className="text-gray-700">
            確認メールを送信しました。
          </p>
          <p className="text-sm text-gray-500">
            <span className="font-medium text-gray-700">{email}</span> 宛にメールをお送りしました。
            メールボックスをご確認の上、記載のリンクからメールアドレスを認証してください。
          </p>
        </div>
        <div className="mt-8">
          <Link href="/login" className="text-sm text-brand-primary hover:underline">
            ログインページへ
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>新規登録</FormTitle>
      
      {error && <ErrorAlert message={error} />}
      
      <form onSubmit={onSubmit} className="space-y-6">
        <NameField value={name} onChange={onNameChange} />
        
        <EmailField value={email} onChange={onEmailChange} />
        
        <PasswordField
          value={password}
          onChange={onPasswordChange}
          helperText="8文字以上・英大文字・英小文字・数字・記号をそれぞれ含む必要があります"
        />
        
        <PasswordField
          value={passwordConfirm}
          onChange={onPasswordConfirmChange}
          id="passwordConfirm"
          label="パスワード（確認）"
        />
        
        <Button
          type="submit"
          disabled={isLoading}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {isLoading ? '登録中...' : '新規登録'}
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
