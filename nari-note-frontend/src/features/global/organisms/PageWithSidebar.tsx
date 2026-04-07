import { ReactNode } from 'react';
import { Sidebar } from './Sidebar';

interface PageWithSidebarProps {
  children: ReactNode;
  maxWidth?: 'wide' | 'full';
}

/**
 * PageWithSidebar - Organism Component
 * 
 * メインコンテンツとサイドバーを含むページレイアウトを提供
 * 記事一覧ページなどで使用
 */
export function PageWithSidebar({ children, maxWidth = 'full' }: PageWithSidebarProps) {
  const widthClass = maxWidth === 'full' ? 'w-full' : 'w-4/5';
  
  return (
    <div className={`${widthClass} mx-auto px-4 py-8`}>
      <div className="flex gap-8">
        <main className="flex-1">
          {children}
        </main>
        <Sidebar />
      </div>
    </div>
  );
}
