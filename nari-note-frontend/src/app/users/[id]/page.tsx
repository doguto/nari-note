'use client';

import { useParams } from 'next/navigation';
import { Header } from '@/components/layout/Header';
import { Sidebar } from '@/components/layout/Sidebar';
import { Footer } from '@/components/layout/Footer';
import { UserProfileContainer } from '@/features/user/containers/UserProfileContainer';

export default function UserProfilePage() {
  const params = useParams();
  const userId = Number(params.id);

  return (
    <div className="min-h-screen bg-gradient-to-b from-brand-bg-gradient-from to-brand-bg-gradient-to">
      <Header />
      
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="flex gap-8">
          <div className="flex-1">
            <UserProfileContainer userId={userId} />
          </div>
          
          <Sidebar />
        </div>
      </div>
      
      <Footer />
    </div>
  );
}
