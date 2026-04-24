import { HeartIcon } from "lucide-react";
import { Button } from "@/components/ui/button";

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
 * 最小単位のコンポーネントとして、いいね状態に応じてボタンを表示
 */
export function LikeButton({ isLiked, likeCount, onClick, disabled = false }: LikeButtonProps) {
  return (
    <Button
      variant="outline"
      onClick={onClick}
      disabled={disabled}
      className="h-auto px-4 py-2 text-black bg-white border-gray-300 hover:bg-gray-50"
    >
      <span>{isLiked ? <HeartIcon className="text-red-500" /> : <HeartIcon className="text-gray-400" />}</span>
      <span className="ml-1">{likeCount}</span>
    </Button>
  );
}
