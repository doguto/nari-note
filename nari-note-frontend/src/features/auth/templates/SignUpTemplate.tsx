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
  onNameChange,
  onEmailChange,
  onPasswordChange,
  onPasswordConfirmChange,
  onSubmit,
}: SignUpTemplateProps) {
  return (
    <div className="w-3/4 w-full mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>新規登録</FormTitle>
      
      {error && <ErrorAlert message={error} />}
      
      <form onSubmit={onSubmit} className="space-y-6">
        <NameField value={name} onChange={onNameChange} />
        
        <EmailField value={email} onChange={onEmailChange} />
        
        <PasswordField 
          value={password} 
          onChange={onPasswordChange}
          helperText="8文字以上で入力してください"
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
