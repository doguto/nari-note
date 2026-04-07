import { Button } from '@/components/ui/button';

interface FollowButtonProps {
  isFollowing: boolean;
  onClick: () => void;
  disabled?: boolean;
}

/**
 * FollowButton - Atom Component
 * 
 * フォロー/フォロー解除ボタン
 * 最小単位のコンポーネントとして、フォロー状態に応じてボタンを表示
 */
export function FollowButton({ isFollowing, onClick, disabled = false }: FollowButtonProps) {
  return (
    <Button
      variant={isFollowing ? 'outline' : 'default'}
      onClick={onClick}
      disabled={disabled}
      className={isFollowing ? '' : 'bg-brand-primary hover:bg-brand-primary-hover'}
    >
      {isFollowing ? 'フォロー解除' : 'フォロー'}
    </Button>
  );
}
