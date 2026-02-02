import { Button } from '@/components/ui/button';

interface CourseFormActionsProps {
  onSave: () => void;
  onOpenPublishSettings: () => void;
  isLoading: boolean;
  isDisabled: boolean;
  isEditMode: boolean;
}

/**
 * CourseFormActions - Organism Component
 * 
 * 講座フォームのアクションボタン群（保存・投稿設定）
 */
export function CourseFormActions({
  onSave,
  onOpenPublishSettings,
  isLoading,
  isDisabled,
  isEditMode,
}: CourseFormActionsProps) {
  return (
    <div className="flex gap-4">
      <Button
        type="button"
        onClick={onSave}
        disabled={isDisabled || isLoading}
        variant="outline"
        className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
      >
        {isLoading ? '保存中...' : '保存'}
      </Button>
      {isEditMode && (
        <Button
          type="button"
          onClick={onOpenPublishSettings}
          disabled={isDisabled || isLoading}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          投稿設定
        </Button>
      )}
    </div>
  );
}
