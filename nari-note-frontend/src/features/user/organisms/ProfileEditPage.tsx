'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { FormTitle, ErrorAlert } from '@/components/common/atoms';
import { 
  UsernameField, 
  BioField, 
  ProfileImageUpload 
} from '@/components/common/molecules';
import { useGetUserProfile, useUpdateUserProfile } from '@/lib/api';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { useAuth } from '@/lib/providers/AuthProvider';

/**
 * ProfileEditPage - Organism Component
 * 
 * プロフィール編集ページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function ProfileEditPage() {
  const router = useRouter();
  const { userId, isLoggedIn, isLoading: authLoading } = useAuth();
  
  const [username, setUsername] = useState('');
  const [bio, setBio] = useState('');
  const [profileImage, setProfileImage] = useState<File | null>(null);
  const [errors, setErrors] = useState<{
    username?: string;
    bio?: string;
    profileImage?: string;
  }>({});
  const [generalError, setGeneralError] = useState<string>();
  const [hasChanges, setHasChanges] = useState(false);
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);

  // 認証チェック - 未ログインの場合はログインページへリダイレクト
  useEffect(() => {
    if (!authLoading && !isLoggedIn) {
      router.push('/login');
    }
  }, [authLoading, isLoggedIn, router]);

  // ユーザー情報取得
  const { data: user, isLoading, error: loadError, refetch } = useGetUserProfile(
    { id: userId || 0 },
    { enabled: !!userId }
  );

  // プロフィール更新
  const updateProfile = useUpdateUserProfile({
    onSuccess: () => {
      router.push(`/users/${userId}`);
    },
    onError: (err) => {
      if (err instanceof Error) {
        setGeneralError(err.message || 'プロフィールの更新に失敗しました');
      } else {
        setGeneralError('プロフィールの更新に失敗しました');
      }
    },
  });

  // 初期データ設定
  useEffect(() => {
    if (user) {
      setUsername(user.username || '');
      setBio(user.bio || '');
    }
  }, [user]);

  // 変更検知
  useEffect(() => {
    if (user) {
      const changed = 
        username !== (user.username || '') ||
        bio !== (user.bio || '') ||
        profileImage !== null;
      setHasChanges(changed);
    }
  }, [username, bio, profileImage, user]);

  const validateForm = () => {
    const newErrors: typeof errors = {};

    // ユーザー名バリデーション
    if (!username.trim()) {
      newErrors.username = 'ユーザー名は必須です';
    } else if (username.length < 3) {
      newErrors.username = 'ユーザー名は3文字以上である必要があります';
    } else if (!/^[a-zA-Z0-9_]+$/.test(username)) {
      newErrors.username = 'ユーザー名は英数字とアンダースコアのみ使用可能です';
    }

    // 自己紹介文バリデーション
    if (bio.length > 250) {
      newErrors.bio = '自己紹介文は250文字以内である必要があります';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setGeneralError(undefined);

    if (!validateForm()) {
      return;
    }

    // TODO: Implement image upload processing later
    // Currently only updating text fields
    updateProfile.mutate({
      name: username,
      bio: bio || undefined,
      profileImage: undefined, // Image upload API needed
    });
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowCancelConfirm(true);
    } else {
      router.push(`/users/${userId}`);
    }
  };

  const handleConfirmCancel = () => {
    router.push(`/users/${userId}`);
  };

  const handleImageSelect = (file: File) => {
    setProfileImage(file);
  };

  const handleImageRemove = () => {
    setProfileImage(null);
  };

  // 認証確認中の表示
  if (authLoading) {
    return <Loading text="認証情報を確認中..." />;
  }

  if (!userId) {
    return null; // リダイレクト中
  }

  if (isLoading) {
    return <Loading text="プロフィール情報を読み込み中..." />;
  }

  if (loadError) {
    return (
      <ErrorMessage 
        message="プロフィール情報の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  return (
    <div className="max-w-2xl mx-auto bg-white rounded-lg shadow-lg p-8">
      <FormTitle>プロフィール編集</FormTitle>

      {generalError && <ErrorAlert message={generalError} />}

      <form onSubmit={handleSubmit} className="space-y-6">
        <ProfileImageUpload
          onImageSelect={handleImageSelect}
          onImageRemove={handleImageRemove}
          error={errors.profileImage}
        />

        <UsernameField
          value={username}
          onChange={setUsername}
          error={errors.username}
        />

        <BioField
          value={bio}
          onChange={setBio}
          error={errors.bio}
        />

        <div className="flex gap-4 pt-4">
          <Button
            type="submit"
            disabled={updateProfile.isPending || !hasChanges}
            className="flex-1 bg-brand-primary hover:bg-brand-primary-hover"
          >
            {updateProfile.isPending ? '保存中...' : '保存'}
          </Button>

          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            disabled={updateProfile.isPending}
            className="flex-1"
          >
            キャンセル
          </Button>
        </div>
      </form>

      {/* キャンセル確認モーダル */}
      {showCancelConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-bold mb-4">変更を破棄しますか？</h3>
            <p className="text-gray-600 mb-6">
              保存していない変更は失われます。
            </p>
            <div className="flex gap-4">
              <Button
                onClick={handleConfirmCancel}
                variant="destructive"
                className="flex-1"
              >
                破棄する
              </Button>
              <Button
                onClick={() => setShowCancelConfirm(false)}
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
