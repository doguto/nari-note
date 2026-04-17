'use client';


import { useParams } from 'next/navigation';
import { UserProfilePage } from '@/features/user/pages';

export default function UserProfilePageRoute() {
  const params = useParams();
  const userId = params.id as string;

  return <UserProfilePage userId={userId} />;
}
