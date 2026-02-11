import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/ui';
import { 
  UsernameField, 
  BioField, 
  ProfileImageUpload 
} from '../organisms';

interface ProfileEditTemplateProps {
  // フォーム値
  username: string;
  bio: string;
  
  // エラー
  errors: {
    username?: string;
    bio?: string;
    profileImage?: string;
  };
  generalError?: string;
  
  // 状態
  hasChanges: boolean;
  isSubmitting: boolean;
  showCancelConfirm: boolean;
  
  // イベントハンドラ
  onUsernameChange: (value: string) => void;
  onBioChange: (value: string) => void;
  onImageSelect: (file: File) => void;
  onImageRemove: () => void;
  onSubmit: (e: React.FormEvent) => void;
  onCancel: () => void;
  onConfirmCancel: () => void;
  onCancelConfirmClose: () => void;
}

/**
 * ProfileEditTemplate - Template Component
 * 
 * プロフィール編集ページのUI構成を担当するテンプレートコンポーネント
 * レイアウトとコンポーネント配置を定義
 */
export function ProfileEditTemplate({
  username,
  bio,
  errors,
  generalError,
  hasChanges,
  isSubmitting,
  showCancelConfirm,
  onUsernameChange,
  onBioChange,
  onImageSelect,
  onImageRemove,
  onSubmit,
  onCancel,
  onConfirmCancel,
  onCancelConfirmClose,
}: ProfileEditTemplateProps) {
  return (
    <div>
      <FormTitle>プロフィール編集</FormTitle>

      {generalError && <ErrorAlert message={generalError} />}

      <form onSubmit={onSubmit} className="space-y-6">
        <ProfileImageUpload
          onImageSelect={onImageSelect}
          onImageRemove={onImageRemove}
          error={errors.profileImage}
        />

        <UsernameField
          value={username}
          onChange={onUsernameChange}
          error={errors.username}
        />

        <BioField
          value={bio}
          onChange={onBioChange}
          error={errors.bio}
        />

        <div className="flex gap-4 pt-4">
          <Button
            type="submit"
            disabled={isSubmitting || !hasChanges}
            className="flex-1 bg-brand-primary hover:bg-brand-primary-hover"
          >
            {isSubmitting ? '保存中...' : '保存'}
          </Button>

          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={isSubmitting}
            className="flex-1"
          >
            キャンセル
          </Button>
        </div>
      </form>

      {/* キャンセル確認モーダル */}
      {showCancelConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-1/2 w-full mx-4">
            <h3 className="text-lg font-bold mb-4">変更を破棄しますか？</h3>
            <p className="text-gray-600 mb-6">
              保存していない変更は失われます。
            </p>
            <div className="flex gap-4">
              <Button
                onClick={onConfirmCancel}
                variant="destructive"
                className="flex-1"
              >
                破棄する
              </Button>
              <Button
                onClick={onCancelConfirmClose}
                variant="outline"
                className="flex-1"
              >
                キャンセル
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
