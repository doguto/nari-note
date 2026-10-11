
import type { Metadata } from 'next';
import { AuthGuard, VerifiedEmailGuard } from '@/features/global/organisms';
import { CourseFormPage } from '@/features/course/pages';

export const metadata: Metadata = {
  robots: {
    index: false,
    follow: false,
  },
};

export default function NewCoursePage() {
  return (
    <AuthGuard redirectPath='/courses/new'>
      <VerifiedEmailGuard>
        <CourseFormPage mode="create" />
      </VerifiedEmailGuard>
    </AuthGuard>
  );
}
