'use client';

import { useCallback } from 'react';
import { useAuth } from '@/lib/providers/AuthProvider';
import { EMAIL_NOT_VERIFIED_CODE, forbiddenHandler } from '@/lib/forbiddenHandler';

export function useRequireVerifiedEmail() {
  const { isLoggedIn, isEmailVerified, isLoading } = useAuth();
  // 未ログインは対象外（401 側のハンドリングに任せる）
  const isBlocked = !isLoading && isLoggedIn && !isEmailVerified;

  const ensureVerifiedEmail = useCallback(() => {
    if (!isBlocked) return true;
    forbiddenHandler.trigger(undefined, EMAIL_NOT_VERIFIED_CODE);
    return false;
  }, [isBlocked]);

  return { isBlocked, isLoading, ensureVerifiedEmail };
}
