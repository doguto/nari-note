import { TextareaField } from '../atoms';
import { CharacterCounter } from './CharacterCounter';

interface BioFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  required?: boolean;
  maxLength?: number;
}

/**
 * BioField - Molecule Component
 * 
 * 自己紹介文入力フィールド
 * 250文字以内
 */
export function BioField({ 
  value, 
  onChange, 
  error,
  required = false,
  maxLength = 250,
}: BioFieldProps) {
  return (
    <div className="space-y-2">
      <TextareaField
        id="bio"
        label="自己紹介"
        value={value}
        onChange={onChange}
        placeholder="自己紹介を入力してください"
        required={required}
        rows={6}
        maxLength={maxLength}
        error={error}
      />
      <CharacterCounter current={value.length} max={maxLength} />
    </div>
  );
}
