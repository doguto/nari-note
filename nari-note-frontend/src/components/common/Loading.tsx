interface LoadingProps {
  size?: 'sm' | 'md' | 'lg';
  text?: string;
}

/**
 * ローディング表示コンポーネント
 * 
 * データ取得中などに表示するローディングインジケーター。
 */
export function Loading({ size = 'md', text = '読み込み中...' }: LoadingProps) {
  const sizeClasses = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  };

  return (
    <div className="flex flex-col items-center justify-center py-8">
      <div 
        className={`animate-spin rounded-full border-b-2 border-brand-primary ${sizeClasses[size]}`}
        role="status"
        aria-label="読み込み中"
      />
      {text && (
        <p className="mt-2 text-gray-600 text-sm">
          {text}
        </p>
      )}
    </div>
  );
}
