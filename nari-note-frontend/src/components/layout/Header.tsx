import Link from 'next/link';

/**
 * ヘッダーコンポーネント
 * 
 * 全ページ共通のヘッダー。
 * ロゴ、ナビゲーション、ユーザーメニューを表示します。
 */
export function Header() {
  return (
    <header className="bg-[#2d3e1f] border-b border-[#1a2515] shadow-sm">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Link href="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <div className="w-10 h-10 bg-[#88b04b] rounded-lg flex items-center justify-center text-white font-bold text-lg">
              な
            </div>
            <span className="text-xl font-bold text-white" style={{ fontFamily: 'serif' }}>
              持稿Qita
            </span>
          </Link>
        </div>
        
        <nav className="hidden md:flex items-center gap-8">
          <Link 
            href="/" 
            className="text-white hover:text-[#88b04b] font-medium transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            祈叩
          </Link>
          <Link 
            href="/articles" 
            className="text-white hover:text-[#88b04b] transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            記を篤る
          </Link>
          <Link 
            href="/submit" 
            className="text-white hover:text-[#88b04b] transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            罰酪する
          </Link>
        </nav>
        
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-gray-600 rounded-full flex items-center justify-center">
              <span className="text-white text-sm">👤</span>
            </div>
            <button className="text-white hover:text-[#88b04b] transition-colors">
              ▼
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}
