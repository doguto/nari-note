import { FormField } from '@/components/ui';

interface XAccountFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
}

export function XAccountField({ value, onChange, error }: XAccountFieldProps) {
  return (
    <div className="space-y-2">
      <FormField
        id="xAccountName"
        label="X（旧Twitter）アカウント"
        type="text"
        value={value}
        onChange={onChange}
        placeholder="@username"
        helperText="@以降のユーザー名を入力すると、プロフィールにXへのリンクが表示されます（空欄で紐付け解除）"
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}
