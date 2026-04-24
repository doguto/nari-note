import { Button } from '@/components/ui/button';

interface TagChipProps {
  tag: string;
  onRemove: (tag: string) => void;
}

/**
 * TagChip - Atom Component
 * 
 * タグを表示する削除可能なチップ
 */
export function TagChip({ tag, onRemove }: TagChipProps) {
  return (
    <span className="px-3 py-1 bg-[var(--brand-bg-light)] text-[var(--brand-text)] rounded-full text-sm flex items-center gap-2">
      #{tag}
      <Button
        type="button"
        variant="ghost"
        onClick={() => onRemove(tag)}
        className="h-auto w-auto p-0 text-red-500 hover:text-red-700 hover:bg-transparent"
        aria-label={`${tag}を削除`}
      >
        ×
      </Button>
    </span>
  );
}
