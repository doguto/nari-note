import { Suspense } from 'react';
import { VerifyEmailPage } from '@/features/auth/pages/VerifyEmailPage';

export default function ValidatePageRoute() {
  return (
    <Suspense>
      <VerifyEmailPage />
    </Suspense>
  );
}
