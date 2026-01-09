'use client';

import { useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { Loading } from '@/components/common/Loading';

/**
 * 記事編集ページ
 * 
 * 既存記事の編集機能（実装予定）
 * 現在は詳細ページにリダイレクト
 */
export default function EditArticlePage() {
  const router = useRouter();
  const params = useParams();
  const { isLoggedIn, isLoading } = useAuth();
  const articleId = params.id;

  useEffect(() => {
    if (!isLoading && !isLoggedIn) {
      router.push('/login');
    }
  }, [isLoggedIn, isLoading, router]);

  useEffect(() => {
    // TODO: 記事編集機能の実装
    // 現在は詳細ページにリダイレクト
    if (!isLoading && isLoggedIn && articleId) {
      router.push(`/articles/${articleId}`);
    }
  }, [isLoading, isLoggedIn, articleId, router]);

  return (
    <div className="min-h-screen bg-gradient-to-b from-[var(--brand-bg-gradient-from)] to-[var(--brand-bg-gradient-to)]">
      <Header />
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loading text="読み込み中..." />
      </div>
      <Footer />
    </div>
  );
}
