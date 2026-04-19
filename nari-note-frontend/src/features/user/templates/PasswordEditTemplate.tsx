import { FormTitle, ErrorAlert, SuccessAlert } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { PasswordChangeFields } from '../organisms';

interface PasswordEditTemplateProps {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
  errors: {
    currentPassword?: string;
    newPassword?: string;
    confirmPassword?: string;
  };
  generalError?: string;
  successMessage?: string;
  isSubmitting: boolean;
  onCurrentPasswordChange: (value: string) => void;
  onNewPasswordChange: (value: string) => void;
  onConfirmPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
}

export function PasswordEditTemplate({
  currentPassword,
  newPassword,
  confirmPassword,
  errors,
  generalError,
  successMessage,
  isSubmitting,
  onCurrentPasswordChange,
  onNewPasswordChange,
  onConfirmPasswordChange,
  onSubmit,
}: PasswordEditTemplateProps) {
  return (
    <div className="max-w-md">
      <FormTitle>パスワード変更</FormTitle>
      {successMessage && <SuccessAlert message={successMessage} />}
      {generalError && <ErrorAlert message={generalError} />}
      <form onSubmit={onSubmit} className="space-y-6">
        <PasswordChangeFields
          currentPassword={currentPassword}
          newPassword={newPassword}
          confirmPassword={confirmPassword}
          errors={errors}
          onCurrentPasswordChange={onCurrentPasswordChange}
          onNewPasswordChange={onNewPasswordChange}
          onConfirmPasswordChange={onConfirmPasswordChange}
        />
        <Button
          type="submit"
          disabled={isSubmitting}
          className="w-full"
          style={{ fontFamily: 'serif' }}
        >
          {isSubmitting ? '変更中...' : 'パスワードを変更する'}
        </Button>
      </form>
    </div>
  );
}
