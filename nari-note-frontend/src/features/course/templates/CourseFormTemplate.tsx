import { PublishSettingsDialog } from '@/features/article/organisms/PublishSettingsDialog';
import { CourseArticleDto } from '@/lib/api/types';
import {
  CourseTitleInput,
  CourseFormActions,
  CourseArticleEditList,
} from '../organisms';

interface CourseFormTemplateProps {
  name: string;
  showPublishDialog: boolean;
  isLoading: boolean;
  isFormDisabled: boolean;
  isEditMode: boolean;
  courseId?: string;
  articles?: CourseArticleDto[];
  onNameChange: (value: string) => void;
  onSave: () => void;
  onOpenPublishSettings: () => void;
  onPublish: (publishedAt?: string) => void;
  onPublishDialogChange: (open: boolean) => void;
}


export function CourseFormTemplate({
  name,
  showPublishDialog,
  isLoading,
  isFormDisabled,
  isEditMode,
  courseId,
  articles,
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
    <form onSubmit={handleSubmit} className="space-y-6 bg-white rounded-lg p-6">
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

      {isEditMode && articles && courseId && (
        <CourseArticleEditList articles={articles} courseId={courseId} />
      )}
    </form>
  );
}
