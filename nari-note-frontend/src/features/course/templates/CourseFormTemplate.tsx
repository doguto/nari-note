'use client';

import { PublishSettingsDialog } from '@/features/article/organisms/PublishSettingsDialog';
import {
  CourseTitleInput,
  CourseFormActions,
} from '../organisms';

interface CourseFormTemplateProps {
  name: string;
  showPublishDialog: boolean;
  isLoading: boolean;
  isFormDisabled: boolean;
  isEditMode: boolean;
  onNameChange: (value: string) => void;
  onSave: () => void;
  onOpenPublishSettings: () => void;
  onPublish: (publishedAt?: string) => void;
  onPublishDialogChange: (open: boolean) => void;
}

/**
 * CourseFormTemplate - Template Component
 * 
 * 講座作成・編集フォームのUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function CourseFormTemplate({
  name,
  showPublishDialog,
  isLoading,
  isFormDisabled,
  isEditMode,
  onNameChange,
  onSave,
  onOpenPublishSettings,
  onPublish,
  onPublishDialogChange,
}: CourseFormTemplateProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <CourseFormActions
        onSave={onSave}
        onOpenPublishSettings={onOpenPublishSettings}
        isLoading={isLoading}
        isDisabled={isFormDisabled}
        isEditMode={isEditMode}
      />

      <CourseTitleInput value={name} onChange={onNameChange} />

      {isEditMode && (
        <PublishSettingsDialog
          open={showPublishDialog}
          onOpenChange={onPublishDialogChange}
          onPublish={onPublish}
          isLoading={isLoading}
        />
      )}
    </form>
  );
}
