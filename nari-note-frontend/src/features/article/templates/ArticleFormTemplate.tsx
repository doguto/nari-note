import { TagInput } from '@/components/molecules';
import { PublishSettingsDialog } from '../organisms/PublishSettingsDialog';
import {
  ArticleTitleInput,
  ArticleBodyEditor,
  ArticleFormActions,
} from '../organisms';
import { LoadingSpinner, ErrorMessage, ErrorAlert } from '@/components/ui';

interface ArticleFormTemplateProps {
  title: string;
  body: string;
  tags: string[];
  maxCharacters: number;
  showPublishDialog: boolean;
  isLoading: boolean;
  isFormDisabled: boolean;
  isLoadingContent?: boolean;
  contentError?: string;
  onRetry?: () => void;
  validationError?: string;
  onTitleChange: (value: string) => void;
  onBodyChange: (value: string) => void;
  onTagsChange: (tags: string[]) => void;
  onSave: () => void;
  onOpenPublishSettings: () => void;
  onPublish: (publishedAt?: string) => void;
  onPublishDialogChange: (open: boolean) => void;
}

/**
 * ArticleFormTemplate - Template Component
 * 
 * 記事作成・編集フォームのUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function ArticleFormTemplate({
  title,
  body,
  tags,
  maxCharacters,
  showPublishDialog,
  isLoading,
  isFormDisabled,
  isLoadingContent,
  contentError,
  onRetry,
  validationError,
  onTitleChange,
  onBodyChange,
  onTagsChange,
  onSave,
  onOpenPublishSettings,
  onPublish,
  onPublishDialogChange,
}: ArticleFormTemplateProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
  };

  // コンテンツ読み込み中
  if (isLoadingContent) {
    return <LoadingSpinner text="記事を読み込み中..." />;
  }

  // コンテンツ取得エラー
  if (contentError) {
    return (
      <ErrorMessage 
        message={contentError} 
        onRetry={onRetry}
      />
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {validationError && <ErrorAlert message={validationError} />}
      
      <ArticleFormActions
        onSave={onSave}
        onOpenPublishSettings={onOpenPublishSettings}
        isLoading={isLoading}
        isDisabled={isFormDisabled}
      />

      <ArticleTitleInput value={title} onChange={onTitleChange} />

      <TagInput tags={tags} onTagsChange={onTagsChange} />

      <ArticleBodyEditor
        value={body}
        onChange={onBodyChange}
        maxCharacters={maxCharacters}
      />

      <PublishSettingsDialog
        open={showPublishDialog}
        onOpenChange={onPublishDialogChange}
        onPublish={onPublish}
        isLoading={isLoading}
      />
    </form>
  );
}
