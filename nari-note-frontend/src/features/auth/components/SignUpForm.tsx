import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/common/atoms';
import { EmailField, PasswordField, NameField } from '@/components/common/molecules';

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

/**
 * サインアップフォーム - Organism Component
 * 
 * ユーザーの新規登録フォームを表示します。
 * Atomic Designパターンに基づいて、Atoms/Moleculesを組み合わせて構築
 */
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
      <FormTitle>新規登録</FormTitle>
      
      {error && <ErrorAlert message={error} />}
      
      <form onSubmit={handleSubmit} className="space-y-4">
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
