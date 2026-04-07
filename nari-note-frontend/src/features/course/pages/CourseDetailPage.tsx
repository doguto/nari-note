'use client';

import { useGetCourseContent } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { PageWithSidebar } from '@/features/global/organisms';
import { CourseDetailTemplate } from '../templates';

interface CourseDetailPageProps {
  courseId: number;
}

/**
 * CourseDetailPage - Page Component
 * 
 * 講座詳細ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function CourseDetailPage({ courseId }: CourseDetailPageProps) {
  const { data: course, isLoading, error, refetch } = useGetCourseContent({ id: courseId });

  if (isLoading) {
    return <LoadingSpinner text="講座を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="講座の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  if (!course) {
    return <ErrorMessage message="講座が見つかりません" />;
  }

  return (
    <PageWithSidebar>
      <CourseDetailTemplate course={course} />
    </PageWithSidebar>
  );
}
