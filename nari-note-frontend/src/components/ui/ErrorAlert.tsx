interface ErrorAlertProps {
  message: string;
}

/**
 * ErrorAlert - Atom Component
 * 
 * エラーメッセージを表示するアラート
 */
export function ErrorAlert({ message }: ErrorAlertProps) {
  return (
    <div className="bg-red-50 border border-red-200 rounded p-3 mb-4" role="alert" aria-live="polite">
      <p className="text-red-600 text-sm">{message}</p>
    </div>
  );
}
