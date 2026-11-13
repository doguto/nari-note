'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { useGetUserProfile, useUpdateUserProfile, useUploadUserIcon } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useRequireVerifiedEmail } from '@/lib/hooks';
import type { GetUserProfileResponse } from '@/lib/api/types';
import { ProfileEditTemplate } from '../templates/ProfileEditTemplate';

const X_ACCOUNT_NAME_PATTERN = /^[a-zA-Z0-9_]{1,15}$/;

// 入力欄では @ 付きで入力されることがあるため、先頭の @ と前後の空白を取り除く
function normalizeXAccountName(value: string): string {
  return value.trim().replace(/^@/, '');
}

interface ProfileEditPageProps {
  initialUserData?: GetUserProfileResponse;
}


export function ProfileEditPage({ initialUserData }: ProfileEditPageProps = {}) {
  const router = useRouter();
  const { userId } = useAuth();
  const { ensureVerifiedEmail } = useRequireVerifiedEmail();

  const { data: fetchedUser, isLoading, error: loadError, refetch } = useGetUserProfile(
    { id: userId || '' },
    { enabled: !!userId && !initialUserData }
  );

  const user = initialUserData || fetchedUser;

  const [username, setUsername] = useState(user?.username || '');
  const [bio, setBio] = useState(user?.bio || '');
  const [xAccountName, setXAccountName] = useState(user?.xAccountName || '');
  const [syncedUser, setSyncedUser] = useState(user);
  const [profileImage, setProfileImage] = useState<File | null>(null);
  const [errors, setErrors] = useState<{
    username?: string;
    bio?: string;
    xAccountName?: string;
    profileImage?: string;
  }>({});
  const [generalError, setGeneralError] = useState<string>();
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);

  // ユーザー情報が取得・更新されたらフォームの値を同期する
  if (user !== syncedUser) {
    setSyncedUser(user);
    if (user) {
      setUsername(user.username || '');
      setBio(user.bio || '');
      setXAccountName(user.xAccountName || '');
    }
  }

  const hasChanges =
    !!user &&
    (username !== (user.username || '') ||
      bio !== (user.bio || '') ||
      normalizeXAccountName(xAccountName) !== (user.xAccountName || '') ||
      profileImage !== null);

  const uploadIcon = useUploadUserIcon();

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

  const validateForm = () => {
    const newErrors: typeof errors = {};

    if (!username.trim()) {
      newErrors.username = 'ユーザー名は必須です';
    } else if (username.length < 3) {
      newErrors.username = 'ユーザー名は3文字以上である必要があります';
    } else if (!/^[a-zA-Z0-9_\-\u3041-\u30FF\u4E00-\u9FFF]+$/.test(username)) {
      newErrors.username = 'ユーザー名は全角文字・半角英数字・アンダーバー・ハイフンのみ使用可能です';
    }

    if (bio.length > 250) {
      newErrors.bio = '自己紹介文は250文字以内である必要があります';
    }

    const normalizedXAccountName = normalizeXAccountName(xAccountName);
    if (normalizedXAccountName && !X_ACCOUNT_NAME_PATTERN.test(normalizedXAccountName)) {
      newErrors.xAccountName = 'Xのユーザー名は半角英数字・アンダーバーの15文字以内で入力してください';
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

    // アイコンアップロードのみメール認証が必要
    if (profileImage && !ensureVerifiedEmail()) return;

    try {
      if (profileImage) {
        await uploadIcon.mutateAsync(profileImage);
      }
      updateProfile.mutate({
        name: username,
        bio: bio || undefined,
        // 空文字列を送ると紐付け解除になる
        xAccountName: normalizeXAccountName(xAccountName),
      });
    } catch (err) {
      if (err instanceof Error) {
        setGeneralError(err.message || 'アイコンのアップロードに失敗しました');
      } else {
        setGeneralError('アイコンのアップロードに失敗しました');
      }
    }
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
    <ProfileEditTemplate
      username={username}
      bio={bio}
      xAccountName={xAccountName}
      currentImage={user?.userIconImageUrl}
      errors={errors}
      generalError={generalError}
      hasChanges={hasChanges}
      isSubmitting={updateProfile.isPending || uploadIcon.isPending}
      showCancelConfirm={showCancelConfirm}
      onUsernameChange={setUsername}
      onBioChange={setBio}
      onXAccountNameChange={setXAccountName}
      onImageSelect={handleImageSelect}
      onImageRemove={handleImageRemove}
      onSubmit={handleSubmit}
      onCancel={handleCancel}
      onConfirmCancel={handleConfirmCancel}
      onCancelConfirmClose={() => setShowCancelConfirm(false)}
    />
  );
}
