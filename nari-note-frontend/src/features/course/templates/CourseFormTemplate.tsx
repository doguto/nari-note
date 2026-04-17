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
    </form>
  );
}
