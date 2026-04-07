import { ReactNode } from 'react';
import { PageContainer } from './PageContainer';
import { PageHeader } from './PageHeader';

interface FormPageLayoutProps {
  title: string;
  description?: string;
  children: ReactNode;
  maxWidth?: 'narrow' | 'medium' | 'wide' | 'full';
}

/**
 * FormPageLayout - Molecule Component
 * 
 * フォームページのレイアウトを提供するコンポーネント
 * ヘッダーと白い背景のフォームコンテナを含む
 */
export function FormPageLayout({ title, description, children, maxWidth = 'wide' }: FormPageLayoutProps) {
  return (
    <PageContainer maxWidth={maxWidth}>
      <PageHeader title={title} description={description} />
      <div className="bg-white rounded-lg shadow-lg p-8">
        {children}
      </div>
    </PageContainer>
  );
}
