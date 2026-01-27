interface CharacterCounterProps {
  count: number;
  max: number;
}

/**
 * CharacterCounter - Molecule Component
 * 
 * 文字数カウンター（表示のみ）
 */
export function CharacterCounter({ count, max }: CharacterCounterProps) {
  const isOverLimit = count > max;
  
  return (
    <span className={`text-sm ${isOverLimit ? 'text-red-500 font-bold' : 'text-gray-600'}`}>
      {count.toLocaleString()} / {max.toLocaleString()} 文字
    </span>
  );
}
