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
  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onCancel()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>認証が必要です</DialogTitle>
          <DialogDescription>
            この操作を行うにはサインインが必要です。サインイン画面に遷移しますか?
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={onCancel}>
            キャンセル
          </Button>
          <Button onClick={onNavigateToSignIn}>
            サインイン画面へ
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
