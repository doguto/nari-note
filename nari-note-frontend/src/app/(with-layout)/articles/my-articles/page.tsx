'use client';

import { AuthGuard } from '@/components/molecules';
import { MyArticlesListPage } from '@/features/article/pages';

/**
 * マイ記事一覧ページ
 * 
 * ログインユーザーの下書き記事と公開済み記事一覧を表示
 */
export default function MyArticlesPage() {
  return (
    <AuthGuard redirectPath="/articles/my-articles">
      <MyArticlesListPage />
    </AuthGuard>
  );
}
