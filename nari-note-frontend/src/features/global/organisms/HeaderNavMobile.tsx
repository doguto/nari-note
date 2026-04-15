import Link from 'next/link';

/**
 * モバイル用ヘッダーナビゲーションコンポーネント
 *
 * スマホ画面でのみ表示される横スクロール対応のナビゲーションバーです。
 */
export function HeaderNavMobile() {
  return (
    <nav className="md:hidden overflow-x-auto border-t border-brand-text-dark">
      <div className="flex items-center gap-8 px-4 py-2.5 w-max">
        <Link
          href="/"
          className="text-white hover:text-brand-primary font-medium transition-colors text-sm whitespace-nowrap"
          style={{ fontFamily: 'serif' }}
        >
          ホーム
        </Link>
        <Link
          href="/search"
          className="text-white hover:text-brand-primary transition-colors text-sm whitespace-nowrap"
          style={{ fontFamily: 'serif' }}
        >
          記事を探す
        </Link>
        <Link
          href="/articles/new"
          className="text-white hover:text-brand-primary transition-colors text-sm whitespace-nowrap"
          style={{ fontFamily: 'serif' }}
        >
          記事を投稿
        </Link>
        <Link
          href="/courses/new"
          className="text-white hover:text-brand-primary transition-colors text-sm whitespace-nowrap"
          style={{ fontFamily: 'serif' }}
        >
          講座を作成
        </Link>
      </div>
    </nav>
  );
}
