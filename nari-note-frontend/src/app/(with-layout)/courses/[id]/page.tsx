import type { Metadata } from 'next';
import { CourseDetailPage } from '@/features/course/pages';
import { getCourseContent } from '@/lib/api/server';

interface CoursePageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: CoursePageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const course = await getCourseContent({ id: Number(id) });
    const description = `${course.userName}による将棋コース「${course.name}」。${course.articles.length}本の記事を収録。`;

    return {
      title: course.name,
      description,
      openGraph: {
        type: 'article',
        title: course.name,
        description,
      },
    };
  } catch {
    return {};
  }
}

export default async function CourseDetailPageRoute({ params }: CoursePageProps) {
  const { id } = await params;

  return <CourseDetailPage courseId={Number(id)} />;
}
