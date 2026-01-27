import { AuthPageLayout } from '@/components/molecules';
import { SignUpPage } from '@/features/auth/pages/SignUpPage';

export default function SignUpPageRoute() {
  return (
    <AuthPageLayout>
      <SignUpPage />
    </AuthPageLayout>
  );
}
