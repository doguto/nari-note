import { HeartIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface LikeButtonProps {
  isLiked: boolean;
  likeCount: number;
  onClick: () => void;
  disabled?: boolean;
}

/**
 * LikeButton - Atom Component
 *
 * いいねボタン
 * いいね済みはハートを塗りつぶし、背景を淡い赤にする
 */
export function LikeButton({ isLiked, likeCount, onClick, disabled = false }: LikeButtonProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-pressed={isLiked}
      aria-label={isLiked ? 'いいねを取り消す' : 'いいね'}
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border px-3.5 py-1.5 text-sm font-semibold tabular-nums transition-colors disabled:opacity-60',
        isLiked
          ? 'border-rose-200 bg-rose-50 text-rose-600 hover:bg-rose-100'
          : 'border-gray-300 bg-white text-gray-600 hover:bg-gray-50',
      )}
    >
      <HeartIcon className={cn('h-4 w-4', isLiked && 'fill-current')} />
      <span>{likeCount}</span>
    </button>
  );
}
