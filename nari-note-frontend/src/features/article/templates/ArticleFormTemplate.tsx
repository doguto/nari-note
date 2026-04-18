import { TagInput } from '@/components/molecules';
import { PublishSettingsDialog, KifuSettingsDialog, KifuList } from '../organisms';
import {
  ArticleTitleInput,
  ArticleBodyEditor,
  ArticleFormActions,
} from '../organisms';
import { LoadingSpinner, ErrorMessage, ErrorAlert } from '@/components/ui';
import type { KifuItem } from '../types/kifu';

interface ArticleFormTemplateProps {
  title: string;
  body: string;
  tags: string[];
  kifuList: KifuItem[];
  editingKifuIndex: number | null;
  maxCharacters: number;
  showPublishDialog: boolean;
  showKifuDialog: boolean;
  isLoading: boolean;
  isFormDisabled: boolean;
  isLoadingContent?: boolean;
  contentError?: string;
  onRetry?: () => void;
  validationError?: string;
  onTitleChange: (value: string) => void;
  onBodyChange: (value: string) => void;
  onTagsChange: (tags: string[]) => void;
  onKifuConfirm: (kifu: KifuItem) => void;
  onKifuEdit: (index: number) => void;
  onKifuDelete: (index: number) => void;
  onSubmit: (e: React.FormEvent) => void;
  onSave: () => void;
  onOpenPublishSettings: () => void;
  onPublish: (publishedAt?: string) => void;
  onPublishDialogChange: (open: boolean) => void;
  onKifuDialogChange: (open: boolean) => void;
}


export function ArticleFormTemplate({
  title,
  body,
  tags,
  kifuList,
  editingKifuIndex,
  maxCharacters,
  showPublishDialog,
  showKifuDialog,
  isLoading,
  isFormDisabled,
  isLoadingContent,
  contentError,
  onRetry,
  validationError,
  onTitleChange,
  onBodyChange,
  onTagsChange,
  onKifuConfirm,
  onKifuEdit,
  onKifuDelete,
  onSubmit,
  onSave,
  onOpenPublishSettings,
  onPublish,
  onPublishDialogChange,
  onKifuDialogChange,
}: ArticleFormTemplateProps) {
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

  const editingKifu = editingKifuIndex !== null ? kifuList[editingKifuIndex] : undefined;
  const defaultName = `棋譜${kifuList.length + 1}`;

  return (
    <form onSubmit={onSubmit} className="space-y-6 bg-white rounded-lg p-6">
      {validationError && <ErrorAlert message={validationError} />}

      <ArticleFormActions
        onSave={onSave}
        onOpenPublishSettings={onOpenPublishSettings}
        isLoading={isLoading}
        isDisabled={isFormDisabled}
      />

      <ArticleTitleInput value={title} onChange={onTitleChange} />

      <TagInput tags={tags} onTagsChange={onTagsChange} />

      <KifuList
        kifuList={kifuList}
        onAdd={() => onKifuDialogChange(true)}
        onEdit={onKifuEdit}
        onDelete={onKifuDelete}
      />

      <ArticleBodyEditor
        value={body}
        onChange={onBodyChange}
        maxCharacters={maxCharacters}
        kifuList={kifuList}
      />

      <PublishSettingsDialog
        open={showPublishDialog}
        onOpenChange={onPublishDialogChange}
        onPublish={onPublish}
        isLoading={isLoading}
      />

      <KifuSettingsDialog
        open={showKifuDialog}
        onOpenChange={onKifuDialogChange}
        onConfirm={onKifuConfirm}
        initialKifu={editingKifu}
        defaultName={editingKifu ? undefined : defaultName}
      />
    </form>
  );
}
