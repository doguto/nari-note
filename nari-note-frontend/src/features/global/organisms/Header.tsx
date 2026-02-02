'use client';

import Link from 'next/link';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useLogout } from '@/lib/api';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu';
import { User, FileText } from 'lucide-react';

/**
 * ヘッダーコンポーネント
 * 
 * 全ページ共通のヘッダー。
 * ロゴ、ナビゲーション、ユーザーメニューを表示します。
 */
export function Header() {
  const { userId, isLoggedIn, isLoading, refetch } = useAuth();
  const logoutMutation = useLogout({
    onSuccess: () => {
      refetch();
    },
  });

  const handleLogout = () => {
    logoutMutation.mutate();
  };

  return (
    <header>
      {/* Top row: Site title with white background */}
      <div className="bg-white border-b border-gray-200">
        <div className="w-11/12 mx-auto px-4 py-3">
          <Link href="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity w-fit">
            <div className="w-10 h-10 bg-brand-primary rounded-lg flex items-center justify-center text-white font-bold text-lg">
              将
            </div>
            <span className="text-lg font-bold text-brand-text" style={{ fontFamily: 'serif' }}>
              将棋ブログ投稿サイト ～なりノート～
            </span>
          </Link>
        </div>
      </div>
      
      {/* Bottom row: Navigation menu with dark background */}
      <div className="bg-brand-text border-b border-brand-text-dark shadow-sm">
        <div className="w-11/12 mx-auto px-4 py-3 flex items-center justify-between">
          <nav className="hidden md:flex items-center gap-8">
            <Link 
              href="/" 
              className="text-white hover:text-brand-primary font-medium transition-colors"
              style={{ fontFamily: 'serif' }}
            >
              ホーム
            </Link>
            <Link 
              href="/articles/search" 
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
          
          <div className="flex items-center gap-4 ml-auto">
            {isLoading ? (
              // ローディング中はスケルトン表示
              <div className="w-24 h-8 bg-brand-text-dark rounded animate-pulse" />
            ) : isLoggedIn ? (
              // ログイン時: マイページメニューとログアウトボタン
              <>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <button
                      className="flex items-center gap-2 text-white hover:text-brand-primary transition-colors"
                      style={{ fontFamily: 'serif' }}
                    >
                      <div className="w-8 h-8 bg-brand-primary rounded-full flex items-center justify-center text-white text-sm font-bold">
                        👤
                      </div>
                      <span>マイページ</span>
                      <svg
                        className="w-4 h-4"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                      </svg>
                    </button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent 
                    align="end" 
                    className="w-56 bg-brand-text border-brand-text-dark"
                  >
                    <DropdownMenuItem asChild>
                      <Link
                        href={`/users/${userId}`}
                        className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2"
                        style={{ fontFamily: 'serif' }}
                      >
                        <User className="w-4 h-4" />
                        <span>マイページ</span>
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator className="bg-brand-text-dark" />
                    <DropdownMenuItem asChild>
                      <Link
                        href="/articles/my-articles"
                        className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2"
                        style={{ fontFamily: 'serif' }}
                      >
                        <FileText className="w-4 h-4" />
                        <span>マイ記事一覧</span>
                      </Link>
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
                <button
                  onClick={handleLogout}
                  disabled={logoutMutation.isPending}
                  className="text-white hover:text-brand-primary transition-colors"
                  style={{ fontFamily: 'serif' }}
                >
                  {logoutMutation.isPending ? 'ログアウト中...' : 'ログアウト'}
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
      </div>
    </header>
  );
}
