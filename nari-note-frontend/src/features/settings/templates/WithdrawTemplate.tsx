import { FormTitle, ErrorAlert } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { PasswordField } from '@/components/molecules';
import { WithdrawConfirmDialog } from '../organisms';

interface WithdrawTemplateProps {
  password: string;
  passwordError?: string;
  generalError?: string;
  isConfirmDialogOpen: boolean;
  isSubmitting: boolean;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  onConfirmDialogOpenChange: (open: boolean) => void;
  onConfirm: () => void;
}

export function WithdrawTemplate({
  password,
  passwordError,
  generalError,
  isConfirmDialogOpen,
  isSubmitting,
  onPasswordChange,
  onSubmit,
  onConfirmDialogOpenChange,
  onConfirm,
}: WithdrawTemplateProps) {
  return (
    <div className="max-w-2xl bg-white rounded-lg p-6">
      <FormTitle>退会</FormTitle>
      <div className="mb-6 rounded-md border border-red-200 bg-red-50 p-4 text-sm text-red-700 space-y-1">
        <p>退会すると、アカウントと以下のデータがすべて削除され、元に戻すことはできません。</p>
        <ul className="list-disc pl-5">
          <li>投稿した記事・講座（下書きを含む）</li>
          <li>コメント・いいね・フォロー</li>
          <li>プロフィール情報・アイコン画像</li>
        </ul>
      </div>
      {generalError && <ErrorAlert message={generalError} />}
      <form onSubmit={onSubmit} className="space-y-6">
        <div>
          <PasswordField
            id="password"
            label="パスワード"
            value={password}
            onChange={onPasswordChange}
            helperText="確認のため現在のパスワードを入力してください"
          />
          {passwordError && (
            <p className="text-sm text-red-500 mt-1">{passwordError}</p>
          )}
        </div>
        <Button
          type="submit"
          variant="destructive"
          size="sm"
          disabled={isSubmitting}
          style={{ fontFamily: 'serif' }}
        >
          退会する
        </Button>
      </form>
      <WithdrawConfirmDialog
        open={isConfirmDialogOpen}
        onOpenChange={onConfirmDialogOpenChange}
        onConfirm={onConfirm}
        isLoading={isSubmitting}
      />
    </div>
  );
}
