interface SuccessAlertProps {
  message: string;
}

export function SuccessAlert({ message }: SuccessAlertProps) {
  return (
    <div className="bg-green-50 border border-green-200 rounded p-3 mb-4" role="status" aria-live="polite">
      <p className="text-green-700 text-sm">{message}</p>
    </div>
  );
}
