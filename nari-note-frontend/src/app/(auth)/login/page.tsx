import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { LoginFormContainer } from '@/features/auth/containers/LoginFormContainer';

export default function LoginPage() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f5f3e8] to-[#e8e4d0] flex flex-col">
      <Header />
      
      <div className="flex-1 flex items-center justify-center py-12 px-4">
        <LoginFormContainer />
      </div>
      
      <Footer />
    </div>
  );
}
