interface FollowStatsProps {
  label: string;
  count: number;
  onClick?: () => void;
}

/**
 * FollowStats - Atom Component
 * 
 * フォロー統計（フォロワー数/フォロー中数）表示
 * クリック可能なスタイルでモーダルを開くトリガーとなる
 */
export function FollowStats({ label, count, onClick }: FollowStatsProps) {
  const classes = onClick 
    ? 'cursor-pointer hover:text-brand-primary transition-colors'
    : '';

  return (
    <div className={classes} onClick={onClick}>
      <span className="font-bold text-brand-text">{count}</span>
      <span className="ml-1">{label}</span>
    </div>
  );
}
