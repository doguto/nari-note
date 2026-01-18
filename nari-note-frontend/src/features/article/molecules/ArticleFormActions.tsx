import { Button } from '@/components/ui/button';

interface ArticleFormActionsProps {
  onSave: () => void;
  onOpenPublishSettings: () => void;
  isLoading: boolean;
  isDisabled: boolean;
}

/**
 * ArticleFormActions - Molecule Component
 * 
 * 記事フォームのアクションボタン群（保存・投稿設定）
 */
export function ArticleFormActions({
  onSave,
  onOpenPublishSettings,
  isLoading,
  isDisabled,
}: ArticleFormActionsProps) {
  return (
    <div className="flex gap-4 pt-4">
      <Button
        type="button"
        onClick={onSave}
        disabled={isDisabled || isLoading}
        variant="outline"
        className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
      >
        {isLoading ? '保存中...' : '保存'}
      </Button>
      <Button
        type="button"
        onClick={onOpenPublishSettings}
        disabled={isDisabled || isLoading}
        className="flex-1 bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
      >
        投稿設定
      </Button>
    </div>
  );
}
