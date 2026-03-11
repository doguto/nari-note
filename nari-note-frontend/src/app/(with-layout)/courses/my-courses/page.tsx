import type { Metadata } from 'next';
import { AuthGuard } from '@/features/global/organisms';
import { MyCoursesListPage } from '@/features/course/pages';

export const metadata: Metadata = {
  robots: {
    index: false,
    follow: false,
  },
};

/**
 * マイ講座一覧ページ
 * 
 * ログインユーザーの講座一覧を表示
 */
export default function MyCoursesPage() {
  return (
    <AuthGuard redirectPath="/courses/my-courses">
      <MyCoursesListPage />
    </AuthGuard>
  );
}
