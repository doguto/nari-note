'use client';


import { AuthGuard } from '@/features/global/organisms';
import { MyArticlesListPage } from '@/features/article/pages';


export default function MyArticlesPage() {
  return (
    <AuthGuard redirectPath="/articles/my-articles">
      <MyArticlesListPage />
    </AuthGuard>
  );
}
