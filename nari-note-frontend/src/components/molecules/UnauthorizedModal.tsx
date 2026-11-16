'use client';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface UnauthorizedModalProps {
  open: boolean;
  title?: string;
  description?: string;
  // 新規ユーザーが多い導線では新規登録を主ボタンにする
  primaryAction?: 'signIn' | 'signUp';
  onNavigateToSignIn: () => void;
  onNavigateToSignUp: () => void;
  onCancel: () => void;
}

const secondaryClassName = 'border-brand-border text-brand-text hover:bg-brand-bg-gradient-to';

export function UnauthorizedModal({
  open,
  title = 'ログインが必要です',
  description = 'この操作を行うにはログインが必要です。アカウントをお持ちでない場合は新規登録してください。',
  primaryAction = 'signIn',
  onNavigateToSignIn,
  onNavigateToSignUp,
  onCancel,
}: UnauthorizedModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onCancel();
    }
  };

  const signUpButton = (
    <Button
      variant={primaryAction === 'signUp' ? 'default' : 'outline'}
      onClick={onNavigateToSignUp}
      className={primaryAction === 'signUp' ? undefined : secondaryClassName}
    >
      新規登録
    </Button>
  );

  const signInButton = (
    <Button
      variant={primaryAction === 'signIn' ? 'default' : 'outline'}
      onClick={onNavigateToSignIn}
      className={primaryAction === 'signIn' ? undefined : secondaryClassName}
    >
      ログイン
    </Button>
  );

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="bg-brand-bg-light border-brand-border"
        // 先頭のキャンセルに初期フォーカスの枠が出て目立つのを避ける
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        <DialogHeader>
          <DialogTitle className="text-brand-text">{title}</DialogTitle>
          <DialogDescription className="text-brand-secondary-text">
            {description}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="gap-2 sm:space-x-0">
          <Button variant="ghost" onClick={onCancel} className="text-brand-secondary-text hover:bg-brand-bg-gradient-to sm:mr-auto">
            キャンセル
          </Button>
          {primaryAction === 'signUp' ? signInButton : signUpButton}
          {primaryAction === 'signUp' ? signUpButton : signInButton}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
