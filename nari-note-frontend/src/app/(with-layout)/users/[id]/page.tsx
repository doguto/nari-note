'use client';

import { useParams } from 'next/navigation';
import { PageWithSidebar } from '@/features/global/organisms';
import { UserProfilePage } from '@/features/user/pages';

export default function UserProfilePageRoute() {
  const params = useParams();
  const userId = Number(params.id);

  return (
    <PageWithSidebar>
      <UserProfilePage userId={userId} />
    </PageWithSidebar>
  );
}
