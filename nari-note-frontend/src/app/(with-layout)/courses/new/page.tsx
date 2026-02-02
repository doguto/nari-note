import { AuthGuard } from '@/components/molecules';
import { CourseFormPage } from '@/features/course/pages';

export default function NewCoursePage() {
  return(
    <AuthGuard redirectPath='/courses/new'>
      <CourseFormPage />
    </AuthGuard>
  );
}
