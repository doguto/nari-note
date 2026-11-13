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
  variant?: 'default' | 'panel';
}

/**
 * EmptyState - Atom Component
 *
 * 空状態表示の最小単位コンポーネント
 * panel は一覧の代わりに置く枠付きの控えめな表示
 */
export function EmptyState({ title, description, icon, action, variant = 'default' }: EmptyStateProps) {
  if (variant === 'panel') {
    return (
      <div className="flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-brand-border bg-white/70 px-4 py-12 text-center">
        {icon && (
          <div className="text-brand-primary-muted [&_svg]:size-10" aria-hidden="true">
            {icon}
          </div>
        )}
        <p className="text-sm font-medium text-gray-600 sm:text-base">{title}</p>
        {description && <p className="text-sm text-gray-500">{description}</p>}
      </div>
    );
  }

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
          className="px-6 py-2 font-medium"
        >
          {action.label}
        </Button>
      )}
    </div>
  );
}
