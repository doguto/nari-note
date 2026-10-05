'use client';

import { useState, useCallback, ReactNode, useEffect } from 'react';
import { ForbiddenModal } from '@/components/molecules';
import { forbiddenHandler } from '@/lib/forbiddenHandler';

export function ForbiddenProvider({ children }: { children: ReactNode }) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [message, setMessage] = useState<string | undefined>(undefined);

  const showForbiddenModal = useCallback((message?: string) => {
    setMessage(message);
    setIsModalOpen(true);
  }, []);

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

  return (
    <>
      {children}
      <ForbiddenModal
        open={isModalOpen}
        message={message}
        onClose={handleClose}
      />
    </>
  );
}
