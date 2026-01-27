import Link from 'next/link';

/**
 * サイドバーコンポーネント
 * 
 * トレンドタグと注目の記事を表示します。
 */
export function Sidebar() {
  return (
    <aside className="w-80 hidden lg:block">
      {/* トレンドセクション */}
      <div className="bg-brand-text rounded-lg p-4 text-white mb-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-bold" style={{ fontFamily: 'serif' }}>
            トレンド
          </h3>
          <span className="text-2xl">🔥</span>
        </div>
        <div className="space-y-2">
          <Link 
            href="/tags/React" 
            className="block text-sm hover:text-brand-primary cursor-pointer transition-colors"
          >
            #React
          </Link>
          <Link 
            href="/tags/TypeScript" 
            className="block text-sm hover:text-brand-primary cursor-pointer transition-colors"
          >
            #TypeScript
          </Link>
          <Link 
            href="/tags/NextJS" 
            className="block text-sm hover:text-brand-primary cursor-pointer transition-colors"
          >
            #Next.js
          </Link>
          <Link 
            href="/tags/Python" 
            className="block text-sm hover:text-brand-primary cursor-pointer transition-colors"
          >
            #Python
          </Link>
          <Link 
            href="/tags/AI" 
            className="block text-sm hover:text-brand-primary cursor-pointer transition-colors"
          >
            #AI
          </Link>
        </div>
      </div>
    </aside>
  );
}
