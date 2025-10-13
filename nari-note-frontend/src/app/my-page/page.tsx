'use client';

import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { currentUser } from '@/lib/mockData';

export default function MyPage() {
  const router = useRouter();

  useEffect(() => {
    // マイページは自分のユーザーページにリダイレクト
    router.replace(`/users/${currentUser.username}`);
  }, [router]);

  return (
    <div className="container mx-auto px-4 py-16 text-center">
      <p className="text-gray-600">リダイレクト中...</p>
    </div>
  );
}
