'use client';

import { createContext, useContext, useState, useCallback, ReactNode, useEffect } from 'react';
import { useRouter, usePathname, useSearchParams } from 'next/navigation';
import { UnauthorizedModal } from '@/components/molecules';
import { unauthorizedHandler } from '@/lib/unauthorizedHandler';

interface UnauthorizedContextType {
  showUnauthorizedModal: () => void;
}

const UnauthorizedContext = createContext<UnauthorizedContextType | undefined>(undefined);

export function UnauthorizedProvider({ children }: { children: ReactNode }) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const showUnauthorizedModal = useCallback(() => {
    setIsModalOpen(true);
  }, []);

  // unauthorizedHandlerにコールバックを登録
  useEffect(() => {
    unauthorizedHandler.register(showUnauthorizedModal);
    return () => {
      unauthorizedHandler.unregister();
    };
  }, [showUnauthorizedModal]);

  const handleNavigateToSignIn = useCallback(() => {
    setIsModalOpen(false);
    // 現在のパスを保存してログイン後に戻れるようにする
    const search = searchParams.toString();
    const currentPath = search ? `${pathname}?${search}` : pathname;
    router.push(`/login?redirect=${encodeURIComponent(currentPath)}`);
  }, [router, pathname, searchParams]);

  const handleCancel = useCallback(() => {
    setIsModalOpen(false);
  }, []);

  return (
    <UnauthorizedContext.Provider value={{ showUnauthorizedModal }}>
      {children}
      <UnauthorizedModal
        open={isModalOpen}
        onNavigateToSignIn={handleNavigateToSignIn}
        onCancel={handleCancel}
      />
    </UnauthorizedContext.Provider>
  );
}

export function useUnauthorized() {
  const context = useContext(UnauthorizedContext);
  if (context === undefined) {
    throw new Error('useUnauthorized must be used within an UnauthorizedProvider');
  }
  return context;
}
