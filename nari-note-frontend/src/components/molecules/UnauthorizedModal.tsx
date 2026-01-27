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
  onCancel: () => void;
}

/**
 * UnauthorizedModal - Molecule Component
 * 
 * 401エラー時にサインイン画面への遷移を確認するモーダル
 */
export function UnauthorizedModal({
  open,
  onNavigateToSignIn,
  onCancel,
}: UnauthorizedModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onCancel();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-brand-bg-light border-brand-border">
        <DialogHeader>
          <DialogTitle className="text-brand-text">認証が必要です</DialogTitle>
          <DialogDescription className="text-brand-secondary-text">
            この操作を行うにはサインインが必要です。サインイン画面に遷移しますか?
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={onCancel} className="border-brand-border text-brand-text hover:bg-brand-bg-gradient-to">
            キャンセル
          </Button>
          <Button onClick={onNavigateToSignIn} className="bg-brand-primary hover:bg-brand-primary-hover">
            サインイン
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
