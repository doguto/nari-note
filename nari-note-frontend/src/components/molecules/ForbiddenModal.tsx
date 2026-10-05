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

interface ForbiddenModalProps {
  open: boolean;
  message?: string;
  onClose: () => void;
}

export function ForbiddenModal({
  open,
  message,
  onClose,
}: ForbiddenModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-brand-bg-light border-brand-border">
        <DialogHeader>
          <DialogTitle className="text-brand-text">権限がありません</DialogTitle>
          <DialogDescription className="text-brand-secondary-text">
            {message || 'この操作を行う権限がありません。'}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button onClick={onClose} className="bg-brand-primary hover:bg-brand-primary-hover">
            閉じる
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
