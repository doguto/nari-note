import { ReactNode } from 'react';
import { Sidebar } from './Sidebar';

interface MainContentSectionProps {
  children: ReactNode;
  title: string;
}

/**
 * MainContentSection - Organism Component
 * 
 * メインコンテンツとサイドバーを含むセクションを表示するコンポーネント
 */
export function MainContentSection({ children, title }: MainContentSectionProps) {
  return (
    <section className="w-11/12 mx-auto px-4 pb-16">
      <div className="flex gap-8">
        {/* メインコンテンツ */}
        <div className="flex-1 mr-2">
          <h2 className="text-2xl font-bold text-brand-text mb-6">{title}</h2>
          {children}
        </div>

        <Sidebar />
      </div>
    </section>
  );
}
