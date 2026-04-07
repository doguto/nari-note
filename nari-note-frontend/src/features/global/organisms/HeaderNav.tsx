import Link from 'next/link';

/**
 * ヘッダーナビゲーションコンポーネント
 *
 * ホーム・記事を探す・記事を投稿・講座を作成のリンクを表示します。
 */
export function HeaderNav() {
  return (
    <nav className="hidden md:flex items-center gap-12">
      <Link
        href="/"
        className="text-white hover:text-brand-primary font-medium transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        ホーム
      </Link>
      <Link
        href="/search"
        className="text-white hover:text-brand-primary transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        記事を探す
      </Link>
      <Link
        href="/articles/new"
        className="text-white hover:text-brand-primary transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        記事を投稿
      </Link>
      <Link
        href="/courses/new"
        className="text-white hover:text-brand-primary transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        講座を作成
      </Link>
    </nav>
  );
}
