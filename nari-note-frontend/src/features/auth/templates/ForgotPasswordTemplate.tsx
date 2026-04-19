import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/ui';
import { EmailField } from '@/components/molecules';

interface ForgotPasswordTemplateProps {
  email: string;
  error?: string;
  isLoading: boolean;
  isSuccess: boolean;
  onEmailChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}

export function ForgotPasswordTemplate({
  email,
  error,
  isLoading,
  isSuccess,
  onEmailChange,
  onSubmit,
}: ForgotPasswordTemplateProps) {
  if (isSuccess) {
    return (
      <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
        <FormTitle>メール送信完了</FormTitle>
        <p className="text-sm text-gray-600 mt-4">
          パスワードリセット用のメールを送信しました。メールをご確認ください。
        </p>
        <div className="mt-8 text-center">
          <Link href="/login" className="text-brand-primary hover:underline text-sm">
            ログインに戻る
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>パスワードをお忘れの方</FormTitle>
      <p className="text-sm text-gray-600 mt-2 mb-6">
        登録済みのメールアドレスを入力してください。パスワードリセット用のメールを送信します。
      </p>

      {error && <ErrorAlert message={error} />}

      <form onSubmit={onSubmit} className="space-y-6">
        <EmailField value={email} onChange={onEmailChange} />

        <Button
          type="submit"
          disabled={isLoading}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {isLoading ? '送信中...' : 'メールを送信'}
        </Button>
      </form>

      <div className="mt-8 text-center">
        <Link href="/login" className="text-brand-primary hover:underline text-sm">
          ログインに戻る
        </Link>
      </div>
    </div>
  );
}
