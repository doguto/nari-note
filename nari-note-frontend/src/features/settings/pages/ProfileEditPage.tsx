'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { useGetUserProfile, useUpdateUserProfile } from '@/lib/api';
import { useAuth } from '@/lib/providers/AuthProvider';
import type { GetUserProfileResponse } from '@/lib/api/types';
import { ProfileEditTemplate } from '../templates/ProfileEditTemplate';

interface ProfileEditPageProps {
  initialUserData?: GetUserProfileResponse;
}


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

  const { data: fetchedUser, isLoading, error: loadError, refetch } = useGetUserProfile(
    { id: userId || '' },
    { enabled: !!userId && !initialUserData }
  );

  const user = initialUserData || fetchedUser;

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

  useEffect(() => {
    if (user) {
      setUsername(user.username || '');
      setBio(user.bio || '');
    }
  }, [user]);

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

    if (!username.trim()) {
      newErrors.username = 'ユーザー名は必須です';
    } else if (username.length < 3) {
      newErrors.username = 'ユーザー名は3文字以上である必要があります';
    } else if (!/^[a-zA-Z0-9_]+$/.test(username)) {
      newErrors.username = 'ユーザー名は英数字とアンダースコアのみ使用可能です';
    }

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
    updateProfile.mutate({
      name: username,
      bio: bio || undefined,
      profileImage: undefined,
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
  );
}
