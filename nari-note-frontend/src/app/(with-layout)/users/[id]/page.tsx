import type { Metadata } from 'next';
import { UserProfilePage } from '@/features/user/pages';
import { getUserProfile } from '@/lib/api/server';
import { buildPageMetadata } from '@/lib/metadata';

interface UserPageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: UserPageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const user = await getUserProfile({ id });
    const description = user.bio?.trim() || `${user.username}さんの将棋記事・講座一覧`;

    return buildPageMetadata({
      title: user.username,
      description,
      path: `/users/${id}`,
      imageUrl: user.userIconImageUrl,
      isSquareImage: true,
      profile: { username: user.username },
    });
  } catch {
    return {};
  }
}

export default async function UserProfilePageRoute({ params }: UserPageProps) {
  const { id } = await params;

  return <UserProfilePage userId={id} />;
}
