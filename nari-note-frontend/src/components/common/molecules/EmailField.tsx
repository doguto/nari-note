import { FormField } from '../atoms';

interface EmailFieldProps {
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
}

/**
 * EmailField - Molecule Component
 * 
 * メールアドレス入力フィールド
 */
export function EmailField({ value, onChange, required = true }: EmailFieldProps) {
  return (
    <FormField
      id="email"
      label="メールアドレス"
      type="email"
      value={value}
      onChange={onChange}
      placeholder="example@email.com"
      required={required}
    />
  );
}
