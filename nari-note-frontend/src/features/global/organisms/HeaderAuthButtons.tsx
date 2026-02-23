import Link from 'next/link';

/**
 * ヘッダー認証ボタンコンポーネント
 *
 * 未ログイン時のログイン・新規登録ボタンを表示します。
 */
export function HeaderAuthButtons() {
  return (
    <>
      <Link
        href="/login"
        className="text-white hover:text-brand-primary transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        ログイン
      </Link>
      <Link
        href="/signup"
        className="px-3 py-1 bg-brand-primary text-white rounded hover:bg-brand-primary-hover transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        新規登録
      </Link>
    </>
  );
}
