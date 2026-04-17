'use client';

import { useParams } from 'next/navigation';
import { AuthGuard } from '@/features/global/organisms';
import { CourseFormPage } from '@/features/course/pages';

export default function EditCoursePage() {
  const params = useParams();
  const courseId = params.id as string;

  return (
    <AuthGuard redirectPath={`/courses/${courseId}/edit`}>
      <CourseFormPage mode="edit" courseId={courseId} />
    </AuthGuard>
  );
}
