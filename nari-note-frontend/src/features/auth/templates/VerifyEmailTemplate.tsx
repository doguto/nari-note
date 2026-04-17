import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/ui';

interface VerifyEmailTemplateProps {
  status: 'loading' | 'success' | 'error';
  error?: string;
}

export function VerifyEmailTemplate({ status, error }: VerifyEmailTemplateProps) {
  return (
    <div className="w-full max-w-md mx-auto bg-white rounded-lg shadow-lg px-10 py-12 text-center">
      <FormTitle>メールアドレスの認証</FormTitle>

      {status === 'loading' && (
        <p className="text-gray-600 mt-4">認証中...</p>
      )}

      {status === 'success' && (
        <div className="mt-4 space-y-6">
          <p className="text-gray-700">メールアドレスの認証が完了しました。</p>
          <Button asChild className="w-full bg-brand-primary hover:bg-brand-primary-hover">
            <Link href="/">ホームへ</Link>
          </Button>
        </div>
      )}

      {status === 'error' && (
        <div className="mt-4 space-y-6">
          {error && <ErrorAlert message={error} />}
          <Button asChild variant="outline" className="w-full">
            <Link href="/signup">新規登録に戻る</Link>
          </Button>
        </div>
      )}
    </div>
  );
}
