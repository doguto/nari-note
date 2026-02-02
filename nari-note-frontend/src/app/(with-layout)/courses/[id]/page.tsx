'use client';

import { useParams } from 'next/navigation';
import { CourseDetailPage } from '@/features/course/pages';

export default function CourseDetailPageRoute() {
  const params = useParams();
  const courseId = Number(params.id);

  return <CourseDetailPage courseId={courseId} />;
}
