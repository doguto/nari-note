'use client';

import Link from 'next/link';
import { useAuth } from '@/lib/providers/AuthProvider';

/**
 * ヘッダーコンポーネント
 * 
 * 全ページ共通のヘッダー。
 * ロゴ、ナビゲーション、ユーザーメニューを表示します。
 */
export function Header() {
  const { userId, isLoggedIn, isLoading, logout } = useAuth();

  return (
    <header className="bg-brand-text border-b border-brand-text-dark shadow-sm">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Link href="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <div className="w-10 h-10 bg-brand-primary rounded-lg flex items-center justify-center text-white font-bold text-lg">
              な
            </div>
            <span className="text-xl font-bold text-white" style={{ fontFamily: 'serif' }}>
              なりノート
            </span>
          </Link>
        </div>
        
        <nav className="hidden md:flex items-center gap-8">
          <Link 
            href="/" 
            className="text-white hover:text-brand-primary font-medium transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            ホーム
          </Link>
          <Link 
            href="/articles" 
            className="text-white hover:text-brand-primary transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            記事を探す
          </Link>
          <Link 
            href="/articles/new" 
            className="text-white hover:text-brand-primary transition-colors"
            style={{ fontFamily: 'serif' }}
          >
            投稿する
          </Link>
        </nav>
        
        <div className="flex items-center gap-4">
          {isLoading ? (
            // ローディング中はスケルトン表示
            <div className="w-24 h-8 bg-brand-text-dark rounded animate-pulse" />
          ) : isLoggedIn ? (
            // ログイン時: マイページへのリンクとログアウトボタン
            <>
              <Link
                href={`/users/${userId}`}
                className="flex items-center gap-2 text-white hover:text-brand-primary transition-colors"
                style={{ fontFamily: 'serif' }}
              >
                <div className="w-8 h-8 bg-brand-primary rounded-full flex items-center justify-center text-white text-sm font-bold">
                  👤
                </div>
                <span>マイページ</span>
              </Link>
              <button
                onClick={logout}
                className="text-white hover:text-brand-primary transition-colors"
                style={{ fontFamily: 'serif' }}
              >
                ログアウト
              </button>
            </>
          ) : (
            // 未ログイン時: ログイン・新規登録ボタン
            <>
              <Link
                href="/login"
                className="text-white hover:text-brand-primary transition-colors"
                style={{ fontFamily: 'serif' }}
              >
                ログイン
              </Link>
              <Link
                href="/signup"
                className="px-4 py-2 bg-brand-primary text-white rounded hover:bg-brand-primary-hover transition-colors"
                style={{ fontFamily: 'serif' }}
              >
                新規登録
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
