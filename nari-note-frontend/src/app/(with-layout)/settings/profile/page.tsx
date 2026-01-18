'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { ProfileEditPage } from '@/features/user/organisms';

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
        <div className="text-gray-500">読み込み中...</div>
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-8 w-full">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-[var(--brand-text)] mb-2">
          プロフィール編集
        </h1>
        <p className="text-gray-600">
          ユーザー名、自己紹介、プロフィール画像を編集できます。変更内容は保存ボタンを押すまで反映されません。
        </p>
      </div>
      
      <div className="bg-white rounded-lg shadow-lg p-8">
        <ProfileEditPage />
      </div>
    </div>
  );
}
