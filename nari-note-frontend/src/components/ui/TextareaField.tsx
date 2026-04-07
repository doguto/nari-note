import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

interface TextareaFieldProps {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  required?: boolean;
  rows?: number;
  maxLength?: number;
  helperText?: string;
  error?: string;
}

/**
 * TextareaField - Atom Component
 * 
 * ラベルとテキストエリアをセットにした基本コンポーネント
 */
export function TextareaField({
  id,
  label,
  value,
  onChange,
  placeholder,
  required = false,
  rows = 4,
  maxLength,
  helperText,
  error,
}: TextareaFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>
        {label}
        {required && <span className="text-red-500">*</span>}
      </Label>
      <Textarea
        id={id}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        required={required}
        rows={rows}
        maxLength={maxLength}
        className={error ? 'border-red-500' : ''}
      />
      {helperText && !error && (
        <p className="text-xs text-gray-500">{helperText}</p>
      )}
      {error && (
        <p className="text-sm text-red-500">{error}</p>
      )}
    </div>
  );
}
