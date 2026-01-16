import { FormField } from '../atoms';

interface UsernameFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  required?: boolean;
}

/**
 * UsernameField - Molecule Component
 * 
 * ユーザー名（username）入力フィールド
 * 英数字とアンダースコアのみ使用可能、3文字以上
 */
export function UsernameField({ 
  value, 
  onChange, 
  error,
  required = true,
}: UsernameFieldProps) {
  return (
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
  );
}
