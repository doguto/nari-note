import type { Metadata } from "next";
import { UserProfilePage } from "@/features/user/pages";
import { getUserProfile } from "@/lib/api/server";

interface UserPageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({
  params,
}: UserPageProps): Promise<Metadata> {
  const { id } = await params;
  const userId = Number(id);

  try {
    const user = await getUserProfile({ id: userId });
    const title = `${user.username} のプロフィール`;
    const description = user.bio || `${user.username} の将棋ブログプロフィールページです。`;

    return {
      title,
      description,
      openGraph: {
        title,
        description,
        type: "profile",
      },
      twitter: {
        card: "summary",
        title,
        description,
      },
    };
  } catch {
    return {
      title: "ユーザープロフィール",
    };
  }
}

export default async function UserProfilePageRoute({
  params,
}: UserPageProps) {
  const { id } = await params;
  const userId = Number(id);

  return <UserProfilePage userId={userId} />;
}
