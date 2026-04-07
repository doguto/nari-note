import { ReactNode } from 'react';

interface PageHeaderProps {
  title: string;
  description?: string;
  action?: ReactNode;
}

/**
 * PageHeader - Molecule Component
 * 
 * ページヘッダーを表示するコンポーネント
 * タイトル、説明、アクションボタンを含む
 */
export function PageHeader({ title, description, action }: PageHeaderProps) {
  return (
    <div className="mb-8">
      <div className="flex items-center justify-between mb-2">
        <h1 className="text-3xl font-bold text-[var(--brand-text)]">
          {title}
        </h1>
        {action && <div>{action}</div>}
      </div>
      {description && (
        <p className="text-gray-600">
          {description}
        </p>
      )}
    </div>
  );
}
