import { AuthGuard } from '@/components/molecules';
import { MyCoursesListPage } from '@/features/course/pages';

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
