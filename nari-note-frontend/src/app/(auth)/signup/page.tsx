import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { SignUpFormContainer } from '@/features/auth/containers/SignUpFormContainer';

export default function SignUpPage() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-brand-bg-gradient-from to-brand-bg-gradient-to flex flex-col">
      <Header />
      
      <div className="flex-1 flex items-center justify-center py-12 px-4">
        <SignUpFormContainer />
      </div>
      
      <Footer />
    </div>
  );
}
