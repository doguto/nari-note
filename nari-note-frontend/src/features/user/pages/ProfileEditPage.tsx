'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { FormPageLayout } from '@/components/molecules';
import { useGetUserProfile, useUpdateUserProfile } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import type { GetUserProfileResponse } from '@/lib/api/types';
import { ProfileEditTemplate } from '../templates/ProfileEditTemplate';

interface ProfileEditPageProps {
  initialUserData?: GetUserProfileResponse;
}

/**
 * ProfileEditPage - Page Component
 * 
 * プロフィール編集ページのビジネスロジックを担当するページコンポーネント
 * データフェッチング、状態管理、バリデーション、イベントハンドリングを行い、Templateにpropsを渡す
 * 
 * @param initialUserData - Optional pre-fetched user data to avoid redundant API calls
 */
export function ProfileEditPage({ initialUserData }: ProfileEditPageProps = {}) {
  const router = useRouter();
  const { userId } = useAuth();
  
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

  // ユーザー情報取得 (initialUserDataがない場合のみフェッチ)
  const { data: fetchedUser, isLoading, error: loadError, refetch } = useGetUserProfile(
    { id: userId || 0 },
    { enabled: !!userId && !initialUserData }
  );

  // Use initialUserData if provided, otherwise use fetched data
  const user = initialUserData || fetchedUser;

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

  // Only show loading if we're actually fetching (not using initialUserData)
  if (!initialUserData && isLoading) {
    return <LoadingSpinner text="プロフィール情報を読み込み中..." />;
  }

  if (!initialUserData && loadError) {
    return (
      <ErrorMessage 
        message="プロフィール情報の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  return (
    <FormPageLayout 
      title="プロフィール編集"
      description="ユーザー名、自己紹介、プロフィール画像を編集できます。変更内容は保存ボタンを押すまで反映されません。"
      maxWidth="medium"
    >
      <ProfileEditTemplate
        username={username}
        bio={bio}
        errors={errors}
        generalError={generalError}
        hasChanges={hasChanges}
        isSubmitting={updateProfile.isPending}
        showCancelConfirm={showCancelConfirm}
        onUsernameChange={setUsername}
        onBioChange={setBio}
        onImageSelect={handleImageSelect}
        onImageRemove={handleImageRemove}
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        onConfirmCancel={handleConfirmCancel}
        onCancelConfirmClose={() => setShowCancelConfirm(false)}
      />
    </FormPageLayout>
  );
}
