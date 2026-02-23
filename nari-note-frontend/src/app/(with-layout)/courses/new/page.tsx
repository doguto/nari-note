import { AuthGuard } from '@/features/global/organisms';
import { CourseFormPage } from '@/features/course/pages';

export default function NewCoursePage() {
  return (
    <AuthGuard redirectPath='/courses/new'>
      <CourseFormPage mode="create" />
    </AuthGuard>
  );
}
