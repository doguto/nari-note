import { LoadingSpinner } from '@/components/ui';
import { Button } from '@/components/ui/button';
import { Plus, BookOpen, Trash2, Edit } from 'lucide-react';
import { CourseDto } from '@/lib/api/types';
import Link from 'next/link';

interface MyCoursesListTemplateProps {
  courses: CourseDto[];
  deletingId: number | null;
  onNewCourse: () => void;
  onDelete: (id: number, name: string) => void;
  onEdit: (id: number) => void;
}

/**
 * MyCoursesListTemplate - Template Component
 * 
 * マイ講座一覧ページのUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function MyCoursesListTemplate({
  courses,
  deletingId,
  onNewCourse,
  onDelete,
  onEdit,
}: MyCoursesListTemplateProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">マイ講座一覧</h1>
        <Button
          onClick={onNewCourse}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          <Plus className="w-4 h-4 mr-2" />
          新規講座作成
        </Button>
      </div>

      <div className="bg-white rounded-lg shadow">
        <div className="p-6">
          {courses.length === 0 ? (
            <div className="text-center py-16">
              <BookOpen className="w-16 h-16 mx-auto text-gray-400 mb-4" />
              <p className="text-gray-500 text-lg mb-6">講座がありません</p>
              <Button
                onClick={onNewCourse}
                className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
              >
                <Plus className="w-4 h-4 mr-2" />
                新規講座を作成
              </Button>
            </div>
          ) : (
            <div className="space-y-4">
              {courses.map((course) => (
                <div
                  key={course.id}
                  className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <Link
                        href={`/courses/${course.id}`}
                        className="text-xl font-bold text-gray-900 hover:text-[var(--brand-primary)] transition-colors"
                      >
                        {course.name || '無題'}
                      </Link>
                      <div className="mt-2 flex items-center gap-4 text-sm text-gray-600">
                        <span>記事数: {course.articleIds?.length || 0}</span>
                        <span>いいね: {course.likeCount || 0}</span>
                      </div>
                      {course.articleNames && course.articleNames.length > 0 && (
                        <div className="mt-2">
                          <p className="text-sm text-gray-500">
                            含まれる記事: {course.articleNames.slice(0, 3).join(', ')}
                            {course.articleNames.length > 3 && ' ...'}
                          </p>
                        </div>
                      )}
                    </div>
                    <div className="flex items-center gap-2 ml-4">
                      <Button
                        onClick={() => onEdit(course.id!)}
                        variant="outline"
                        size="sm"
                        className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
                      >
                        <Edit className="w-4 h-4 mr-1" />
                        編集
                      </Button>
                      <Button
                        onClick={() => onDelete(course.id!, course.name || '無題')}
                        variant="outline"
                        size="sm"
                        className="border-red-500 text-red-500 hover:bg-red-50"
                      >
                        <Trash2 className="w-4 h-4 mr-1" />
                        削除
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {deletingId !== null && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg">
            <LoadingSpinner text="削除中..." />
          </div>
        </div>
      )}
    </div>
  );
}
