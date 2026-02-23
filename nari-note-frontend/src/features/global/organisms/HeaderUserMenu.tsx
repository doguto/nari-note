import Link from 'next/link';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu';
import { UserAvatar } from '@/components/ui';
import { User, FileText, BookOpen } from 'lucide-react';

interface HeaderUserMenuProps {
  userId: string;
  userName: string;
  onLogout: () => void;
  isLoggingOut: boolean;
}

/**
 * ヘッダーユーザーメニューコンポーネント
 *
 * ログイン時のマイページドロップダウンメニューとログアウトボタンを表示します。
 */
export function HeaderUserMenu({ userId, userName, onLogout, isLoggingOut }: HeaderUserMenuProps) {
  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <button
            className="flex items-center gap-2 text-white hover:text-brand-primary transition-colors text-sm"
            style={{ fontFamily: 'serif' }}
          >
            <UserAvatar username={userName} size="sm" />
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
              className="cursor-pointer hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2 text-sm"
              style={{ fontFamily: 'serif' }}
            >
              <User className="w-4 h-4" />
              <span className="text-white">マイページ</span>
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
        onClick={onLogout}
        disabled={isLoggingOut}
        className="text-white hover:text-brand-primary transition-colors text-sm"
        style={{ fontFamily: 'serif' }}
      >
        {isLoggingOut ? 'ログアウト中...' : 'ログアウト'}
      </button>
    </>
  );
}
