import { TextareaField } from '@/components/common/atoms/TextareaField';
import { CharacterCounter } from '@/components/common/molecules/CharacterCounter';

interface CommentFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  maxLength?: number;
}

/**
 * CommentField - Molecule Component
 * 
 * コメント入力フィールド（TextareaFieldと文字数カウンターを組み合わせ）
 */
export function CommentField({ 
  value, 
  onChange, 
  error,
  maxLength = 1000,
}: CommentFieldProps) {
  return (
    <div className="space-y-2">
      <TextareaField
        id="comment"
        label="コメント"
        value={value}
        onChange={onChange}
        placeholder="コメントを入力してください"
        error={error}
        required
        rows={4}
        maxLength={maxLength}
      />
      <div className="flex justify-end">
        <CharacterCounter count={value.length} max={maxLength} />
      </div>
    </div>
  );
}
