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

interface WithdrawConfirmDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
  isLoading: boolean;
}

export function WithdrawConfirmDialog({
  open,
  onOpenChange,
  onConfirm,
  isLoading,
}: WithdrawConfirmDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle style={{ fontFamily: 'serif' }}>本当に退会しますか？</DialogTitle>
          <DialogDescription>
            この操作は取り消せません。投稿した記事・講座・コメントなどのデータはすべて削除されます。
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isLoading}
          >
            キャンセル
          </Button>
          <Button
            type="button"
            variant="destructive"
            onClick={onConfirm}
            disabled={isLoading}
          >
            {isLoading ? '退会処理中...' : '退会する'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
