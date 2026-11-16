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
  onNavigateToSignIn: () => void;
  onNavigateToSignUp: () => void;
  onCancel: () => void;
}

export function UnauthorizedModal({
  open,
  onNavigateToSignIn,
  onNavigateToSignUp,
  onCancel,
}: UnauthorizedModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onCancel();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="bg-brand-bg-light border-brand-border"
        // 先頭のキャンセルに初期フォーカスの枠が出て目立つのを避ける
        onOpenAutoFocus={(e) => e.preventDefault()}
      >
        <DialogHeader>
          <DialogTitle className="text-brand-text">ログインが必要です</DialogTitle>
          <DialogDescription className="text-brand-secondary-text">
            この操作を行うにはログインが必要です。アカウントをお持ちでない場合は新規登録してください。
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="gap-2 sm:space-x-0">
          <Button variant="ghost" onClick={onCancel} className="text-brand-secondary-text hover:bg-brand-bg-gradient-to sm:mr-auto">
            キャンセル
          </Button>
          <Button variant="outline" onClick={onNavigateToSignUp} className="border-brand-border text-brand-text hover:bg-brand-bg-gradient-to">
            新規登録
          </Button>
          <Button onClick={onNavigateToSignIn}>
            ログイン
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
