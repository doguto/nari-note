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
import { UserAvatar } from '@/components/ui';
import { User, FileText, BookOpen } from 'lucide-react';

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
      {/* Top row: Site title with ochre/tan background */}
      <div className="bg-brand-bg-light border-b border-brand-border">
        <div className="w-11/12 mx-auto px-4 py-1">
          <Link href="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity w-fit">
            <div className="w-7 h-7 bg-brand-primary rounded-lg flex items-center justify-center text-white font-bold text-sm">
              将
            </div>
            <span className="text-sm font-bold text-brand-text" style={{ fontFamily: 'serif' }}>
              将棋ブログ投稿サイト ～なりノート～
            </span>
          </Link>
        </div>
      </div>
      
      {/* Bottom row: Navigation menu with dark background */}
      <div className="bg-brand-text border-b border-brand-text-dark shadow-sm">
        <div className="w-11/12 mx-auto px-4 py-2.5 flex items-center justify-center relative">
          <nav className="hidden md:flex items-center gap-12">
            <Link 
              href="/" 
              className="text-white hover:text-brand-primary font-medium transition-colors text-sm"
              style={{ fontFamily: 'serif' }}
            >
              ホーム
            </Link>
            <Link 
              href="/articles/search" 
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
          
          <div className="flex items-center gap-4 absolute right-4">
            {isLoading ? (
              // ローディング中はスケルトン表示
              <div className="w-24 h-6 bg-brand-text-dark rounded animate-pulse" />
            ) : isLoggedIn ? (
              // ログイン時: マイページメニューとログアウトボタン
              <>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <button
                      className="flex items-center gap-2 text-white hover:text-brand-primary transition-colors text-sm"
                      style={{ fontFamily: 'serif' }}
                    >
                      <UserAvatar username="User" size="sm" />
                      <span>マイページ</span>
                      <svg
                        className="w-3 h-3"
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
                        className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2 text-sm"
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
                        className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2 text-sm"
                        style={{ fontFamily: 'serif' }}
                      >
                        <FileText className="w-4 h-4" />
                        <span>マイ記事一覧</span>
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link
                        href="/courses/my-courses"
                        className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2 text-sm"
                        style={{ fontFamily: 'serif' }}
                      >
                        <BookOpen className="w-4 h-4" />
                        <span>マイ講座一覧</span>
                      </Link>
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
                <button
                  onClick={handleLogout}
                  disabled={logoutMutation.isPending}
                  className="text-white hover:text-brand-primary transition-colors text-sm"
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
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
