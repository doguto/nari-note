import type { Metadata } from "next";
import { TagArticleListPage } from "@/features/tag/pages";

interface TagPageProps {
  params: Promise<{ name: string }>;
}

export async function generateMetadata({
  params,
}: TagPageProps): Promise<Metadata> {
  const { name } = await params;
  const decodedName = decodeURIComponent(name);
  const title = `#${decodedName} の記事一覧`;
  const description = `将棋の「${decodedName}」タグが付いた記事の一覧です。`;

  return {
    title,
    description,
    openGraph: {
      title,
      description,
    },
    twitter: {
      card: "summary",
      title,
      description,
    },
  };
}

export default async function TagPage({ params }: TagPageProps) {
  const { name } = await params;

  return <TagArticleListPage tagName={name} />;
}
