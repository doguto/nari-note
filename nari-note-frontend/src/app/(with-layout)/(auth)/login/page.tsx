import { AuthPageLayout } from '@/components/molecules';
import { LoginPage } from '@/features/auth/pages/LoginPage';

export default function LoginPageRoute() {
  return (
    <AuthPageLayout>
      <LoginPage />
    </AuthPageLayout>
  );
}
