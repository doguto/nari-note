
import type { Metadata } from 'next';
import { CourseDetailPage } from '@/features/course/pages';
import { getCourseContent } from '@/lib/api/server';
import { buildPageMetadata } from '@/lib/metadata';

interface CoursePageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: CoursePageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const { course } = await getCourseContent({ id });
    const description = `${course.userName}による将棋コース「${course.name}」。${course.articles.length}本の記事を収録。`;

    return buildPageMetadata({
      title: course.name,
      description,
      path: `/courses/${id}`,
      article: {
        publishedTime: course.publishedAt,
        authors: [course.userName],
      },
    });
  } catch {
    return {};
  }
}

export default async function CourseDetailPageRoute({ params }: CoursePageProps) {
  const { id } = await params;

  return <CourseDetailPage courseId={id} />;
}
