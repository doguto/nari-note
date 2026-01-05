import { ReactNode } from 'react';

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
 * 空状態表示コンポーネント
 * 
 * データが存在しない場合に表示します。
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
        <p className="text-gray-600 text-center max-w-md mb-6">
          {description}
        </p>
      )}
      
      {action && (
        <button
          onClick={action.onClick}
          className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939] transition-colors font-medium"
        >
          {action.label}
        </button>
      )}
    </div>
  );
}
