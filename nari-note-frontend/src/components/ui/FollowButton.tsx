import { UserCheck, UserMinus, UserPlus } from 'lucide-react';
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
 * フォロー中はホバー時のみ「フォロー解除」を表示する
 */
export function FollowButton({ isFollowing, onClick, disabled = false }: FollowButtonProps) {
  if (!isFollowing) {
    return (
      <Button onClick={onClick} disabled={disabled}>
        <UserPlus />
        フォロー
      </Button>
    );
  }

  return (
    <Button
      onClick={onClick}
      disabled={disabled}
      aria-label="フォロー中（押すとフォロー解除）"
      className="group bg-brand-primary-light text-brand-primary-dark font-bold hover:bg-red-50 hover:text-red-700"
    >
      <span className="inline-flex items-center gap-2 group-hover:hidden">
        <UserCheck />
        フォロー中
      </span>
      <span className="hidden items-center gap-2 group-hover:inline-flex">
        <UserMinus />
        フォロー解除
      </span>
    </Button>
  );
}
