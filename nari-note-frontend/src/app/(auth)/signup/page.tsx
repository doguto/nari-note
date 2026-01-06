import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { SignUpFormContainer } from '@/features/auth/containers/SignUpFormContainer';

/**
 * 新規登録ページ
 * 
 * ユーザーの新規登録フォームを表示します。
 */
export default function SignUpPage() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f5f3e8] to-[#e8e4d0] flex flex-col">
      <Header />
      
      <div className="flex-1 flex items-center justify-center py-12 px-4">
        <SignUpFormContainer />
      </div>
      
      <Footer />
    </div>
  );
}
