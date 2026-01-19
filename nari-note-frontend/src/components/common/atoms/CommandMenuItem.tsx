import { LucideIcon } from 'lucide-react';

interface CommandMenuItemProps {
  icon: LucideIcon;
  label: string;
  description: string;
  onClick: () => void;
  isSelected?: boolean;
}

/**
 * CommandMenuItem - Atom Component
 * 
 * スラッシュコマンドメニューの個別アイテム
 */
export function CommandMenuItem({
  icon: Icon,
  label,
  description,
  onClick,
  isSelected = false,
}: CommandMenuItemProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`w-full flex items-start gap-3 px-4 py-3 text-left hover:bg-gray-100 transition-colors ${
        isSelected ? 'bg-gray-100' : ''
      }`}
    >
      <Icon className="w-5 h-5 mt-0.5 text-gray-600 flex-shrink-0" />
      <div className="flex-1 min-w-0">
        <div className="font-medium text-sm text-gray-900">{label}</div>
        <div className="text-xs text-gray-500">{description}</div>
      </div>
    </button>
  );
}
