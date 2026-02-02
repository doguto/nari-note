import { ReactNode } from 'react';

interface PageContainerProps {
  children: ReactNode;
  maxWidth?: 'narrow' | 'medium' | 'wide' | 'full';
}

/**
 * PageContainer - Molecule Component
 * 
 * ページのコンテナを提供するコンポーネント
 * 共通のレイアウト（余白、最大幅）を適用
 */
export function PageContainer({ children, maxWidth = 'wide' }: PageContainerProps) {
  const widthClasses = {
    narrow: 'w-3/5',
    medium: 'w-4/5',
    wide: 'w-11/12',
    full: 'w-full'
  };

  return (
    <div className={`${widthClasses[maxWidth]} mx-auto px-4 py-8`}>
      {children}
    </div>
  );
}
