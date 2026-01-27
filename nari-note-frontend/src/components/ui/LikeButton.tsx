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
    <button
      onClick={onClick}
      disabled={disabled}
      className={`flex items-center gap-2 px-4 py-2 rounded transition-colors ${
        isLiked
          ? 'bg-pink-500 text-white hover:bg-pink-600'
          : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
      } ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
    >
      <span>{isLiked ? '❤️' : '🤍'}</span>
      <span>{likeCount}</span>
    </button>
  );
}
