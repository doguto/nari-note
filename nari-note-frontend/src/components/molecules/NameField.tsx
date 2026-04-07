import { FormField } from '@/components/ui';

interface NameFieldProps {
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
}

/**
 * NameField - Molecule Component
 * 
 * ユーザー名入力フィールド
 */
export function NameField({ value, onChange, required = true }: NameFieldProps) {
  return (
    <FormField
      id="name"
      label="ユーザー名"
      type="text"
      value={value}
      onChange={onChange}
      placeholder="山田太郎"
      required={required}
    />
  );
}
