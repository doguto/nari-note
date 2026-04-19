import { PasswordField } from '@/components/molecules';

interface PasswordChangeFieldsProps {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
  errors: {
    currentPassword?: string;
    newPassword?: string;
    confirmPassword?: string;
  };
  onCurrentPasswordChange: (value: string) => void;
  onNewPasswordChange: (value: string) => void;
  onConfirmPasswordChange: (value: string) => void;
}

export function PasswordChangeFields({
  currentPassword,
  newPassword,
  confirmPassword,
  errors,
  onCurrentPasswordChange,
  onNewPasswordChange,
  onConfirmPasswordChange,
}: PasswordChangeFieldsProps) {
  return (
    <div className="space-y-4">
      <div>
        <PasswordField
          id="currentPassword"
          label="現在のパスワード"
          value={currentPassword}
          onChange={onCurrentPasswordChange}
        />
        {errors.currentPassword && (
          <p className="text-sm text-red-500 mt-1">{errors.currentPassword}</p>
        )}
      </div>
      <div>
        <PasswordField
          id="newPassword"
          label="新しいパスワード"
          value={newPassword}
          onChange={onNewPasswordChange}
          helperText="8文字以上で入力してください"
        />
        {errors.newPassword && (
          <p className="text-sm text-red-500 mt-1">{errors.newPassword}</p>
        )}
      </div>
      <div>
        <PasswordField
          id="confirmPassword"
          label="新しいパスワード（確認）"
          value={confirmPassword}
          onChange={onConfirmPasswordChange}
        />
        {errors.confirmPassword && (
          <p className="text-sm text-red-500 mt-1">{errors.confirmPassword}</p>
        )}
      </div>
    </div>
  );
}
