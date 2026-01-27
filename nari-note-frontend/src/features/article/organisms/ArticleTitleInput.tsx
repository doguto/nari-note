import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';

interface ArticleTitleInputProps {
  value: string;
  onChange: (value: string) => void;
}

/**
 * ArticleTitleInput - Molecule Component
 * 
 * 記事タイトル入力フィールド
 */
export function ArticleTitleInput({ value, onChange }: ArticleTitleInputProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor="title">
        タイトル <span className="text-red-500">*</span>
      </Label>
      <Input
        id="title"
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="記事のタイトルを入力してください"
        required
      />
    </div>
  );
}
