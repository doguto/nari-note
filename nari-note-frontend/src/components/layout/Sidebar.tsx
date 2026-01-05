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
      <div className="bg-[#2d3e1f] rounded-lg p-4 text-white mb-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-bold" style={{ fontFamily: 'serif' }}>
            トレンド
          </h3>
          <span className="text-2xl">🔥</span>
        </div>
        <div className="space-y-2">
          <Link 
            href="/tags/React" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #React
          </Link>
          <Link 
            href="/tags/TypeScript" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #TypeScript
          </Link>
          <Link 
            href="/tags/NextJS" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #Next.js
          </Link>
          <Link 
            href="/tags/Python" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #Python
          </Link>
          <Link 
            href="/tags/AI" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #AI
          </Link>
        </div>
      </div>

      {/* 注目の記事セクション */}
      <div className="bg-[#f5f3e8] rounded-lg p-4 border border-[#d4cdb3]">
        <h3 className="text-lg font-bold text-[#2d3e1f] mb-4" style={{ fontFamily: 'serif' }}>
          注目の記事
        </h3>
        <div className="space-y-3">
          {[
            { id: 1, image: '👤', label: 'プログラミング入門', count: '471いいね' },
            { id: 2, image: '🎮', label: 'ゲーム開発', count: '356いいね' },
            { id: 3, image: '📱', label: 'Web開発', count: '289いいね' }
          ].map((item) => (
            <Link 
              key={item.id} 
              href={`/articles/${item.id}`} 
              className="flex items-center gap-2 hover:opacity-80 transition-opacity"
            >
              <div className="w-12 h-12 bg-[#e8e4d0] rounded flex items-center justify-center text-xl flex-shrink-0">
                {item.image}
              </div>
              <div className="flex-1 text-sm">
                <div className="text-[#555]">{item.label}</div>
                <div className="text-gray-400">{item.count}</div>
              </div>
            </Link>
          ))}
        </div>
      </div>

      {/* フッター */}
      <div className="mt-6 text-center text-sm text-gray-500">
        © 2024 なりノート
      </div>
    </aside>
  );
}
