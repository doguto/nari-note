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
      <button
        type="button"
        onClick={() => onRemove(tag)}
        className="text-red-500 hover:text-red-700"
        aria-label={`${tag}を削除`}
      >
        ×
      </button>
    </span>
  );
}
