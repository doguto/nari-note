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
            トランド
          </h3>
          <span className="text-2xl">🔥</span>
        </div>
        <div className="space-y-2">
          <Link 
            href="/tags/trend" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            テランド
          </Link>
          <Link 
            href="/tags/角換貨" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #角換貨
          </Link>
          <Link 
            href="/tags/厚井古天" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #厚井古天
          </Link>
          <Link 
            href="/tags/定齢" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #定齢
          </Link>
          <Link 
            href="/tags/AI折翼" 
            className="block text-sm hover:text-[#88b04b] cursor-pointer transition-colors"
          >
            #AI折翼
          </Link>
        </div>
      </div>

      {/* 注目の記事セクション */}
      <div className="bg-[#f5f3e8] rounded-lg p-4 border border-[#d4cdb3]">
        <h3 className="text-lg font-bold text-[#2d3e1f] mb-4" style={{ fontFamily: 'serif' }}>
          注倒の議記
        </h3>
        <div className="space-y-3">
          {[
            { image: '👤', label: '鬼賓值 #無直敢', count: '471萬雨' },
            { image: '🎮', label: '紬任', count: '011 6001' },
            { image: '🎮', label: '紬任', count: '18萬' }
          ].map((item, index) => (
            <Link 
              key={index} 
              href="/articles/1" 
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
        © 2024刊伦 プライセンポトレブ
      </div>
    </aside>
  );
}
