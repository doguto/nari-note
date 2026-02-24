import type { Metadata } from "next";
import { ArticleDetailPage } from "@/features/article/pages";
import { getArticleContent } from "@/lib/api/server";

interface ArticlePageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({
  params,
}: ArticlePageProps): Promise<Metadata> {
  const { id } = await params;
  const articleId = Number(id);

  try {
    const article = await getArticleContent({ id: articleId });
    const description = article.body
      ? article.body.replace(/<[^>]*>/g, "").slice(0, 160)
      : undefined;

    return {
      title: article.title,
      description,
      openGraph: {
        title: article.title,
        description,
        type: "article",
        publishedTime: article.publishedAt,
        modifiedTime: article.updatedAt,
        authors: [article.authorName],
        tags: article.tags,
      },
      twitter: {
        card: "summary_large_image",
        title: article.title,
        description,
      },
    };
  } catch {
    return {
      title: "記事",
    };
  }
}

export default async function ArticleDetailPageRoute({
  params,
}: ArticlePageProps) {
  const { id } = await params;
  const articleId = Number(id);

  return <ArticleDetailPage articleId={articleId} />;
}
