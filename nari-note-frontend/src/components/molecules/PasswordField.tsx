import { FormField } from '@/components/ui';

interface PasswordFieldProps {
  value: string;
  onChange: (value: string) => void;
  id?: string;
  label?: string;
  required?: boolean;
  helperText?: string;
}

/**
 * PasswordField - Molecule Component
 * 
 * パスワード入力フィールド
 */
export function PasswordField({
  value,
  onChange,
  id = 'password',
  label = 'パスワード',
  required = true,
  helperText,
}: PasswordFieldProps) {
  return (
    <FormField
      id={id}
      label={label}
      type="password"
      value={value}
      onChange={onChange}
      placeholder="••••••••"
      required={required}
      minLength={8}
      helperText={helperText}
    />
  );
}
