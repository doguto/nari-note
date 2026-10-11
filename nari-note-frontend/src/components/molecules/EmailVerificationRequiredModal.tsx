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

interface EmailVerificationRequiredModalProps {
  open: boolean;
  message?: string;
  isResending: boolean;
  isResent: boolean;
  resendErrorMessage?: string;
  onResend: () => void;
  onClose: () => void;
}

export function EmailVerificationRequiredModal({
  open,
  message,
  isResending,
  isResent,
  resendErrorMessage,
  onResend,
  onClose,
}: EmailVerificationRequiredModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-brand-bg-light border-brand-border">
        <DialogHeader>
          <DialogTitle className="text-brand-text">メールアドレスの認証が必要です</DialogTitle>
          <DialogDescription className="text-brand-secondary-text">
            {message || 'この操作を行うには、メールアドレスの認証が必要です。'}
          </DialogDescription>
        </DialogHeader>
        {isResent && (
          <p className="text-sm text-brand-text">
            確認メールを送信しました。メール内のリンクから認証を完了してください。
          </p>
        )}
        {resendErrorMessage && (
          <p className="text-sm text-red-600">{resendErrorMessage}</p>
        )}
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            閉じる
          </Button>
          <Button
            onClick={onResend}
            disabled={isResending || isResent}
            className="bg-brand-primary hover:bg-brand-primary-hover"
          >
            {isResending ? '送信中...' : '確認メールを再送する'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
