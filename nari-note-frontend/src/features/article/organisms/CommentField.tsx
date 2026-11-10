import { Textarea } from '@/components/ui/textarea';
import { cn } from '@/lib/utils';

interface CommentFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  maxLength?: number;
}

/**
 * CommentField - Molecule Component
 *
 * コメント入力フィールド
 */
export function CommentField({
  value,
  onChange,
  error,
  maxLength = 1000,
}: CommentFieldProps) {
  return (
    <div className="space-y-1">
      <Textarea
        id="comment"
        aria-label="コメント"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="コメントを書く"
        required
        rows={2}
        maxLength={maxLength}
        className={cn('min-h-16 resize-y focus:min-h-24', error && 'border-red-500')}
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}
