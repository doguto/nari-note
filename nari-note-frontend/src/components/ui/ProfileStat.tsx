interface ProfileStatProps {
  label: string;
  count: number;
  onClick: () => void;
}

/**
 * ProfileStat - Atom Component
 *
 * プロフィールの件数（記事数・フォロワー数など）を表示し、押すと対応する一覧へ切り替える
 * スマホでは数値とラベルを縦に積んだマス、PC では横並びのテキストとして表示する
 */
export function ProfileStat({ label, count, onClick }: ProfileStatProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex flex-col items-center py-2.5 text-gray-600 transition-colors hover:text-brand-text sm:flex-row sm:items-baseline sm:gap-1 sm:py-0"
    >
      <span className="text-[17px] font-bold text-brand-text sm:text-xl">{count.toLocaleString()}</span>
      <span className="text-[11px] sm:text-sm">{label}</span>
    </button>
  );
}
