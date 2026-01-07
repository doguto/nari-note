interface CharacterCounterProps {
  count: number;
  max: number;
}

/**
 * CharacterCounter - Molecule Component
 * 
 * 文字数カウンター
 */
export function CharacterCounter({ count, max }: CharacterCounterProps) {
  const isOverLimit = count > max;
  
  return (
    <div className="space-y-2">
      <span className={`text-sm ${isOverLimit ? 'text-red-500 font-bold' : 'text-gray-600'}`}>
        {count.toLocaleString()} / {max.toLocaleString()} 文字
      </span>
      {isOverLimit && (
        <p className="text-sm text-red-500">
          文字数が上限を超えています。{max.toLocaleString()}文字以内に収めてください。
        </p>
      )}
    </div>
  );
}
