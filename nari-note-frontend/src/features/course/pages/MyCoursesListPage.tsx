'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useGetMyCourses, useDeleteCourse } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { PageWithSidebar } from '@/features/global/organisms';
import { MyCoursesListTemplate } from '../templates/MyCoursesListTemplate';

/**
 * MyCoursesListPage - Page Component
 * 
 * マイ講座一覧ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function MyCoursesListPage() {
  const router = useRouter();
  const { userId } = useAuth();
  const [deletingId, setDeletingId] = useState<string | null>(null);
  
  const { data, isLoading, error, refetch } = useGetMyCourses(
    { enabled: !!userId }
  );
  
  const deleteCourse = useDeleteCourse({
    onSuccess: () => {
      setDeletingId(null);
      refetch();
    },
    onError: (error) => {
      console.error('講座の削除に失敗しました:', error);
      alert('講座の削除に失敗しました。もう一度お試しください。');
      setDeletingId(null);
    },
  });

  const handleDelete = (id: string, name: string) => {
    if (window.confirm(`「${name}」を削除してもよろしいですか？`)) {
      setDeletingId(id);
      deleteCourse.mutate({ id });
    }
  };

  const handleNewCourse = () => {
    router.push('/courses/new');
  };

  const handleEdit = (id: string) => {
    router.push(`/courses/${id}/edit`);
  };

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

  const myCourses = data?.courses || [];

  return (
    <PageWithSidebar>
      <MyCoursesListTemplate
        courses={myCourses}
        onDelete={handleDelete}
        onEdit={handleEdit}
        onNewCourse={handleNewCourse}
        deletingId={deletingId}
      />
    </PageWithSidebar>
  );
}
