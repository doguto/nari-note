'use client';

import { useParams } from 'next/navigation';
import { AuthGuard, VerifiedEmailGuard } from '@/features/global/organisms';
import { CourseFormPage } from '@/features/course/pages';

export default function EditCoursePage() {
  const params = useParams();
  const courseId = params.id as string;

  return (
    <AuthGuard redirectPath={`/courses/${courseId}/edit`}>
      <VerifiedEmailGuard>
        <CourseFormPage mode="edit" courseId={courseId} />
      </VerifiedEmailGuard>
    </AuthGuard>
  );
}
