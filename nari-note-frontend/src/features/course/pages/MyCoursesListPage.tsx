'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/providers/AuthProvider';
import { useGetMyCourses, useDeleteCourse } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { PageWithSidebar } from '@/features/global/organisms';
import { DeleteConfirmModal } from '@/components/molecules/DeleteConfirmModal';
import { MyCoursesListTemplate } from '../templates/MyCoursesListTemplate';

export function MyCoursesListPage() {
  const router = useRouter();
  const { userId } = useAuth();
  const [activeTab, setActiveTab] = useState<'published' | 'drafts'>('published');
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<{ id: string; name: string } | null>(null);

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

  const handleDeleteRequest = (id: string, name: string) => {
    setPendingDelete({ id, name });
  };

  const handleConfirmDelete = () => {
    if (!pendingDelete) return;
    setDeletingId(pendingDelete.id);
    setPendingDelete(null);
    deleteCourse.mutate({ id: pendingDelete.id });
  };

  const handleCancelDelete = () => {
    setPendingDelete(null);
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

  const allCourses = data?.courses || [];
  const publishedCourses = allCourses.filter(course => course.isPublished);
  const draftCourses = allCourses.filter(course => !course.isPublished);

  return (
    <PageWithSidebar>
      <MyCoursesListTemplate
        activeTab={activeTab}
        publishedCourses={publishedCourses}
        draftCourses={draftCourses}
        deletingId={deletingId}
        onTabChange={setActiveTab}
        onDelete={handleDeleteRequest}
        onEdit={handleEdit}
        onNewCourse={handleNewCourse}
      />
      <DeleteConfirmModal
        open={pendingDelete !== null}
        articleTitle={pendingDelete?.name ?? ''}
        onConfirm={handleConfirmDelete}
        onCancel={handleCancelDelete}
      />
    </PageWithSidebar>
  );
}
