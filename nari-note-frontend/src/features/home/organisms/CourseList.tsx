import { CourseCard } from '@/components/molecules';
import { EmptyState, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { CourseDto } from '@/lib/api/types';

interface CourseListProps {
  courses: CourseDto[];
  isLoading: boolean;
  error: Error | null;
  onRetry: () => void;
}

/**
 * CourseList - Organism Component
 * 
 * 講座一覧を表示するOrganismコンポーネント
 * ローディング、エラー、空状態のハンドリングを含む
 */
export function CourseList({ courses, isLoading, error, onRetry }: CourseListProps) {
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

  if (courses.length === 0) {
    return <EmptyState title="まだ講座がありません" />;
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
          articleCount={course.articleIds?.length ?? 0}
          likeCount={course.likeCount ?? 0}
        />
      ))}
    </div>
  );
}
