import { ReactNode } from 'react';
import { CourseCard } from '@/components/molecules';
import { EmptyState, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { CourseDto } from '@/lib/api/types';

interface CourseListProps {
  courses?: CourseDto[];
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
  emptyMessage?: string;
  emptyIcon?: ReactNode;
  emptyVariant?: 'default' | 'panel';
  showAuthor?: boolean;
}


export function CourseList({
  courses,
  isLoading,
  error,
  onRetry,
  emptyMessage = 'まだ講座がありません',
  emptyIcon,
  emptyVariant = 'default',
  showAuthor = true,
}: CourseListProps) {
  if (isLoading) {
    return <LoadingSpinner text="講座を読み込み中..." />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="講座の取得に失敗しました" 
        onRetry={onRetry}
      />
    );
  }

  if (!courses || courses.length === 0) {
    if (emptyVariant === 'panel') {
      return <EmptyState variant="panel" title={emptyMessage} icon={emptyIcon} />;
    }
    return <EmptyState title={emptyMessage} />;
  }

  return (
    <div className="space-y-4">
      {courses.map((course) => (
        <CourseCard
          key={course.id}
          id={course.id!}
          name={course.name ?? ''}
          userId={course.userId ?? 0}
          userName={course.userName ?? '不明なユーザー'}
          userIconImageUrl={course.userIconImageUrl}
          articleCount={course.articleIds?.length ?? 0}
          likeCount={course.likeCount ?? 0}
          showAuthor={showAuthor}
        />
      ))}
    </div>
  );
}
