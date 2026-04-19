import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { UserAvatar } from '@/components/ui';
import { User, FileText, BookOpen, LogOut, Settings } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface HeaderUserMenuProps {
  userId: string;
  userName: string;
  profileImage?: string;
  onLogout: () => void;
  isLoggingOut: boolean;
}


export function HeaderUserMenu({ userId, userName, profileImage, onLogout, isLoggingOut }: HeaderUserMenuProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const router = useRouter();

  const handleLogoutConfirm = () => {
    setIsModalOpen(false);
    onLogout();
    router.push('/');
  };

  return (
    <>
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle style={{ fontFamily: 'serif' }}>ログアウトしますか？</DialogTitle>
            <DialogDescription style={{ fontFamily: 'serif' }}>
              ログアウトするとマイページへのアクセスには再度ログインが必要です。
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:gap-0">
            <Button
              variant="outline"
              onClick={() => setIsModalOpen(false)}
              disabled={isLoggingOut}
              style={{ fontFamily: 'serif' }}
            >
              キャンセル
            </Button>
            <Button
              onClick={handleLogoutConfirm}
              disabled={isLoggingOut}
              className="bg-red-600 hover:bg-red-700 text-white"
              style={{ fontFamily: 'serif' }}
            >
              <LogOut className="w-4 h-4 mr-2" />
              {isLoggingOut ? 'ログアウト中...' : 'ログアウト'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            className="flex items-center gap-2 text-white hover:bg-white/10 hover:text-white transition-colors text-sm"
            style={{ fontFamily: 'serif' }}
          >
            <UserAvatar username={userName} profileImage={profileImage} size="sm" />
            <span>マイページ</span>
            <svg
              className="w-3 h-3"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </Button>
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
          <DropdownMenuSeparator className="bg-brand-text-dark" />
          <DropdownMenuItem asChild>
            <Link
              href="/settings/general/profile"
              className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2 text-sm"
              style={{ fontFamily: 'serif' }}
            >
              <Settings className="w-4 h-4" />
              <span>設定</span>
            </Link>
          </DropdownMenuItem>
          <DropdownMenuSeparator className="bg-brand-text-dark" />
          <DropdownMenuItem
            onClick={() => setIsModalOpen(true)}
            disabled={isLoggingOut}
            className="cursor-pointer text-white hover:text-brand-primary hover:bg-brand-text-hover transition-colors flex items-center gap-2 text-sm"
            style={{ fontFamily: 'serif' }}
          >
            <LogOut className="w-4 h-4" />
            <span>{isLoggingOut ? 'ログアウト中...' : 'ログアウト'}</span>
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  );
}
