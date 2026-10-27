import type { Metadata } from 'next';
import { UserProfilePage } from '@/features/user/pages';
import { getUserProfile } from '@/lib/api/server';

interface UserPageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: UserPageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const user = await getUserProfile({ id });
    const description = user.bio?.trim()
      ? user.bio.trim().slice(0, 160)
      : `${user.username}さんの将棋記事・講座一覧`;

    return {
      title: user.username,
      description,
      openGraph: {
        type: 'profile',
        title: user.username,
        description,
        username: user.username,
        images: user.userIconImageUrl ? [user.userIconImageUrl] : undefined,
      },
    };
  } catch {
    return {};
  }
}

export default async function UserProfilePageRoute({ params }: UserPageProps) {
  const { id } = await params;

  return <UserProfilePage userId={id} />;
}
