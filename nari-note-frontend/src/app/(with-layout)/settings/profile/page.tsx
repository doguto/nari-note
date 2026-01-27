'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { LoadingSpinner } from '@/components/ui';
import { FormPageLayout } from '@/components/molecules';
import { ProfileEditPage } from '@/features/user/pages';

/**
 * プロフィール編集ページ
 * 
 * ユーザーがプロフィール情報を編集できるページ
 */
export default function SettingsProfilePage() {
  const router = useRouter();
  const { isLoggedIn, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isLoggedIn) {
      router.push('/login');
    }
  }, [isLoggedIn, isLoading, router]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <LoadingSpinner text="読み込み中..." />
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return (
    <FormPageLayout 
      title="プロフィール編集"
      description="ユーザー名、自己紹介、プロフィール画像を編集できます。変更内容は保存ボタンを押すまで反映されません。"
      maxWidth="medium"
    >
      <ProfileEditPage />
    </FormPageLayout>
  );
}
