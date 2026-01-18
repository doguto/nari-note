'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
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
      <div className="min-h-screen bg-gradient-to-b from-[var(--brand-bg-gradient-from)] to-[var(--brand-bg-gradient-to)] flex flex-col">
        <Header />
        <div className="flex-1 flex items-center justify-center">
          <div className="text-gray-500">読み込み中...</div>
        </div>
        <Footer />
      </div>
    );
  }

  if (!isLoggedIn) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-[var(--brand-bg-gradient-from)] to-[var(--brand-bg-gradient-to)] flex flex-col">
      <Header />
      
      <div className="flex-1 max-w-5xl mx-auto px-4 py-8 w-full">
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
      
      <Footer />
    </div>
  );
}
