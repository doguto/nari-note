import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/ui';
import { PasswordField } from '@/components/molecules';

interface ResetPasswordTemplateProps {
  password: string;
  confirmPassword: string;
  error?: string;
  isLoading: boolean;
  isSuccess: boolean;
  isTokenMissing: boolean;
  onPasswordChange: (value: string) => void;
  onConfirmPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}

export function ResetPasswordTemplate({
  password,
  confirmPassword,
  error,
  isLoading,
  isSuccess,
  isTokenMissing,
  onPasswordChange,
  onConfirmPasswordChange,
  onSubmit,
}: ResetPasswordTemplateProps) {
  if (isTokenMissing) {
    return (
      <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
        <FormTitle>無効なリンク</FormTitle>
        <p className="text-sm text-gray-600 mt-4">
          パスワードリセットリンクが無効または期限切れです。再度お試しください。
        </p>
        <div className="mt-8 text-center">
          <Link href="/forgot-password" className="text-brand-primary hover:underline text-sm">
            パスワードリセットをやり直す
          </Link>
        </div>
      </div>
    );
  }

  if (isSuccess) {
    return (
      <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
        <FormTitle>パスワード更新完了</FormTitle>
        <p className="text-sm text-gray-600 mt-4">
          パスワードが正常に更新されました。新しいパスワードでログインしてください。
        </p>
        <div className="mt-8 text-center">
          <Link href="/login" className="text-brand-primary hover:underline text-sm">
            ログインへ
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12">
      <FormTitle>パスワードの再設定</FormTitle>
      <p className="text-sm text-gray-600 mt-2 mb-6">
        新しいパスワードを入力してください。
      </p>

      {error && <ErrorAlert message={error} />}

      <form onSubmit={onSubmit} className="space-y-6">
        <PasswordField
          value={password}
          onChange={onPasswordChange}
          label="新しいパスワード"
        />

        <PasswordField
          value={confirmPassword}
          onChange={onConfirmPasswordChange}
          label="新しいパスワード（確認）"
          id="confirmPassword"
        />

        <Button
          type="submit"
          disabled={isLoading}
          className="w-full bg-brand-primary hover:bg-brand-primary-hover"
        >
          {isLoading ? '更新中...' : 'パスワードを更新'}
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
