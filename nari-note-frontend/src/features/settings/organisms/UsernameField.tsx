import { FormField } from '@/components/ui';

interface UsernameFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  required?: boolean;
}

export function UsernameField({
  value,
  onChange,
  error,
  required = true,
}: UsernameFieldProps) {
  return (
    <div className="space-y-2">
      <FormField
        id="username"
        label="ユーザー名"
        type="text"
        value={value}
        onChange={onChange}
        placeholder="user_name123"
        required={required}
        minLength={3}
        helperText="英数字とアンダースコアのみ使用可能（3文字以上）"
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}
