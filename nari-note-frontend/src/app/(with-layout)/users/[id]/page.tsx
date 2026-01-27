'use client';

import { useParams } from 'next/navigation';
import { Sidebar } from '@/features/global/organisms/Sidebar';
import { UserProfilePage } from '@/features/user/organisms';

export default function UserProfilePageRoute() {
  const params = useParams();
  const userId = Number(params.id);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 w-full">
      <div className="flex gap-8">
        <div className="flex-1">
          <UserProfilePage userId={userId} />
        </div>
        
        <Sidebar />
      </div>
    </div>
  );
}
