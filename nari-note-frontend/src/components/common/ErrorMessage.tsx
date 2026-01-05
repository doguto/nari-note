interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

/**
 * エラーメッセージ表示コンポーネント
 * 
 * APIエラーなどのエラー状態を表示します。
 */
export function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  return (
    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
      <div className="flex items-start gap-3">
        <div className="flex-shrink-0">
          <svg 
            className="h-5 w-5 text-red-400" 
            viewBox="0 0 20 20" 
            fill="currentColor"
            aria-hidden="true"
          >
            <path 
              fillRule="evenodd" 
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" 
              clipRule="evenodd" 
            />
          </svg>
        </div>
        <div className="flex-1">
          <p className="text-red-700 font-medium mb-1">
            エラーが発生しました
          </p>
          <p className="text-red-600 text-sm">
            {message}
          </p>
        </div>
      </div>
      
      {onRetry && (
        <button
          onClick={onRetry}
          className="mt-3 px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition-colors text-sm font-medium"
        >
          再試行
        </button>
      )}
    </div>
  );
}
