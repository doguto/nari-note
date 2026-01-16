import { FormField } from '../atoms';

interface DisplayNameFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  required?: boolean;
}

/**
 * DisplayNameField - Molecule Component
 * 
 * 表示名入力フィールド
 * 20文字以内
 */
export function DisplayNameField({ 
  value, 
  onChange, 
  error,
  required = false,
}: DisplayNameFieldProps) {
  return (
    <FormField
      id="displayName"
      label="表示名"
      type="text"
      value={value}
      onChange={onChange}
      placeholder="山田太郎"
      required={required}
      helperText="20文字以内"
    />
  );
}
