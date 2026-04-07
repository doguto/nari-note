import { MetadataRoute } from 'next';
import { getArticles } from '@/lib/api/server';

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://nari-note.com';

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticRoutes: MetadataRoute.Sitemap = [
    {
      url: siteUrl,
      lastModified: new Date(),
      changeFrequency: 'daily',
      priority: 1,
    },
    {
      url: `${siteUrl}/search`,
      lastModified: new Date(),
      changeFrequency: 'weekly',
      priority: 0.5,
    },
  ];

  try {
    const { articles } = await getArticles({ limit: 1000, offset: 0 });
    const articleRoutes: MetadataRoute.Sitemap = articles
      .filter((a) => a.isPublished)
      .map((article) => ({
        url: `${siteUrl}/articles/${article.id}`,
        lastModified: new Date(article.updatedAt),
        changeFrequency: 'weekly' as const,
        priority: 0.8,
      }));

    return [...staticRoutes, ...articleRoutes];
  } catch {
    return staticRoutes;
  }
}
