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
  layout?: 'list' | 'grid';
  showAuthor?: boolean;
}

// 1件だけのときに右半分が空かないよう横幅いっぱいに広げる
const GRID_CLASS = 'grid gap-4 md:grid-cols-2 md:[&>:only-child]:col-span-2';


export function CourseList({
  courses,
  isLoading,
  error,
  onRetry,
  emptyMessage = 'まだ講座がありません',
  emptyIcon,
  layout = 'list',
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
    if (layout === 'grid') {
      return <EmptyState variant="panel" title={emptyMessage} icon={emptyIcon} />;
    }
    return <EmptyState title={emptyMessage} />;
  }

  return (
    <div className={layout === 'grid' ? GRID_CLASS : 'space-y-4'}>
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
