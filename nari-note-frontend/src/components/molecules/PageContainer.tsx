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
    narrow: 'max-w-3xl',
    medium: 'max-w-5xl',
    wide: 'max-w-6xl',
    full: 'max-w-7xl w-full'
  };

  return (
    <div className={`${widthClasses[maxWidth]} mx-auto px-4 py-8`}>
      {children}
    </div>
  );
}
