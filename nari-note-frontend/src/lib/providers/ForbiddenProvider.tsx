'use client';

import { useState, useCallback, ReactNode, useEffect } from 'react';
import { EmailVerificationRequiredModal, ForbiddenModal } from '@/components/molecules';
import { useResendVerificationEmail } from '@/lib/api/hooks';
import { EMAIL_NOT_VERIFIED_CODE, forbiddenHandler } from '@/lib/forbiddenHandler';

export function ForbiddenProvider({ children }: { children: ReactNode }) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [message, setMessage] = useState<string | undefined>(undefined);
  const [code, setCode] = useState<string | undefined>(undefined);
  const resendMutation = useResendVerificationEmail();
  const { reset: resetResend } = resendMutation;

  const showForbiddenModal = useCallback((message?: string, code?: string) => {
    setMessage(message);
    setCode(code);
    resetResend();
    setIsModalOpen(true);
  }, [resetResend]);

  // forbiddenHandlerにコールバックを登録
  useEffect(() => {
    forbiddenHandler.register(showForbiddenModal);
    return () => {
      forbiddenHandler.unregister();
    };
  }, [showForbiddenModal]);

  const handleClose = useCallback(() => {
    setIsModalOpen(false);
  }, []);

  const isEmailNotVerified = code === EMAIL_NOT_VERIFIED_CODE;

  return (
    <>
      {children}
      <ForbiddenModal
        open={isModalOpen && !isEmailNotVerified}
        message={message}
        onClose={handleClose}
      />
      <EmailVerificationRequiredModal
        open={isModalOpen && isEmailNotVerified}
        message={message}
        isResending={resendMutation.isPending}
        isResent={resendMutation.isSuccess}
        resendErrorMessage={resendMutation.error?.message}
        onResend={() => resendMutation.mutate()}
        onClose={handleClose}
      />
    </>
  );
}
