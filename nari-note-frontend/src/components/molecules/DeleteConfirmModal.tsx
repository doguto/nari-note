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

interface DeleteConfirmModalProps {
  open: boolean;
  articleTitle: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export function DeleteConfirmModal({
  open,
  articleTitle,
  onConfirm,
  onCancel,
}: DeleteConfirmModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onCancel();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>記事を削除しますか？</DialogTitle>
          <DialogDescription>
            「{articleTitle}」を削除します。この操作は取り消せません。
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={onCancel}>
            キャンセル
          </Button>
          <Button
            onClick={onConfirm}
            className="bg-red-500 hover:bg-red-600 text-white"
          >
            削除する
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
