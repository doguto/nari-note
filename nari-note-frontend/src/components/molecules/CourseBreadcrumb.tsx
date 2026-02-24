import Link from 'next/link';
import { BookOpen, ChevronRight } from 'lucide-react';

interface CourseBreadcrumbProps {
  courseId: number;
  courseName: string;
}

/**
 * CourseBreadcrumb - Molecule Component
 *
 * コースに属する記事のパンくずリストを表示するコンポーネント
 */
export function CourseBreadcrumb({ courseId, courseName }: CourseBreadcrumbProps) {
  return (
    <div className="mb-4 flex items-center gap-2 text-sm text-gray-600">
      <BookOpen className="w-4 h-4" />
      <Link
        href={`/courses/${courseId}`}
        className="hover:text-brand-primary hover:underline flex items-center gap-1"
      >
        {courseName}
      </Link>
      <ChevronRight className="w-4 h-4" />
      <span className="text-gray-800 font-medium">この記事</span>
    </div>
  );
}
