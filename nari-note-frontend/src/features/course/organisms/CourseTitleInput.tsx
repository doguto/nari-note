import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';

interface CourseTitleInputProps {
  value: string;
  onChange: (value: string) => void;
}

/**
 * CourseTitleInput - Organism Component
 * 
 * 講座名入力フィールド
 */
export function CourseTitleInput({ value, onChange }: CourseTitleInputProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor="course-title">
        講座名 <span className="text-red-500">*</span>
      </Label>
      <Input
        id="course-title"
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="講座名を入力してください"
        required
        maxLength={100}
      />
    </div>
  );
}
