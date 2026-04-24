import { ReactNode } from 'react';
import { Button } from '@/components/ui/button';

interface EmptyStateProps {
  title: string;
  description?: string;
  icon?: ReactNode;
  action?: {
    label: string;
    onClick: () => void;
  };
}

/**
 * EmptyState - Atom Component
 * 
 * 空状態表示の最小単位コンポーネント
 * データが存在しない場合に表示します
 */
export function EmptyState({ title, description, icon, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 px-4">
      {icon && (
        <div className="text-6xl mb-4" aria-hidden="true">
          {icon}
        </div>
      )}
      
      <h3 className="text-xl font-bold text-gray-800 mb-2 text-center">
        {title}
      </h3>
      
      {description && (
        <p className="text-gray-600 text-center w-1/2 mb-6">
          {description}
        </p>
      )}
      
      {action && (
        <Button
          onClick={action.onClick}
          className="px-6 py-2 bg-brand-primary text-white hover:bg-brand-primary-hover font-medium"
        >
          {action.label}
        </Button>
      )}
    </div>
  );
}
