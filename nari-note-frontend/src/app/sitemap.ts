import { MetadataRoute } from 'next';
import { getArticles, getCourses, getPopularTags } from '@/lib/api/server';
import type { ArticleThumbnailDto, CourseDto } from '@/lib/api/types';

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://nari-note.com';

const PAGE_SIZE = 100;
// 無限ループ防止のための取得上限ページ数（PAGE_SIZE × MAX_PAGES 件まで取得）
const MAX_PAGES = 500;

async function fetchAll<T>(fetchPage: (limit: number, offset: number) => Promise<T[]>): Promise<T[]> {
  const items: T[] = [];
  for (let page = 0; page < MAX_PAGES; page++) {
    const pageItems = await fetchPage(PAGE_SIZE, page * PAGE_SIZE);
    items.push(...pageItems);
    if (pageItems.length < PAGE_SIZE) break;
  }
  return items;
}

async function fetchAllArticles(): Promise<ArticleThumbnailDto[]> {
  return fetchAll(async (limit, offset) => (await getArticles({ limit, offset })).articles);
}

async function fetchAllCourses(): Promise<CourseDto[]> {
  return fetchAll(async (limit, offset) => (await getCourses({ limit, offset })).courses);
}

async function fetchPopularTagNames(): Promise<string[]> {
  const { tags } = await getPopularTags();
  return tags.map((t) => t.name);
}

function latestDate(current: Date | undefined, candidate: Date): Date {
  return current && current > candidate ? current : candidate;
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticRoutes: MetadataRoute.Sitemap = [
    {
      url: siteUrl,
      lastModified: new Date(),
      changeFrequency: 'daily',
      priority: 1,
    },
    {
      url: `${siteUrl}/terms`,
      changeFrequency: 'yearly',
      priority: 0.1,
    },
    {
      url: `${siteUrl}/privacy-policy`,
      changeFrequency: 'yearly',
      priority: 0.1,
    },
  ];

  // 1つのAPIが失敗しても他のURLは出力できるよう、個別に失敗を許容する
  const [articlesResult, coursesResult, popularTagsResult] = await Promise.allSettled([
    fetchAllArticles(),
    fetchAllCourses(),
    fetchPopularTagNames(),
  ]);
  const articles = articlesResult.status === 'fulfilled' ? articlesResult.value.filter((a) => a.isPublished) : [];
  const courses = coursesResult.status === 'fulfilled' ? coursesResult.value.filter((c) => c.isPublished) : [];
  const popularTagNames = popularTagsResult.status === 'fulfilled' ? popularTagsResult.value : [];

  const articleRoutes: MetadataRoute.Sitemap = articles.map((article) => ({
    url: `${siteUrl}/articles/${article.id}`,
    lastModified: new Date(article.updatedAt),
    priority: 0.8,
  }));

  // 講座内の記事は記事一覧APIに含まれないため、講座から収集する
  const singleArticleIds = new Set(articles.map((a) => a.id));
  const courseArticleIds = new Set(
    courses.flatMap((c) => c.articleIds).filter((id) => !singleArticleIds.has(id))
  );
  const courseArticleRoutes: MetadataRoute.Sitemap = [...courseArticleIds].map((id) => ({
    url: `${siteUrl}/articles/${id}`,
    priority: 0.7,
  }));

  const courseRoutes: MetadataRoute.Sitemap = courses.map((course) => ({
    url: `${siteUrl}/courses/${course.id}`,
    lastModified: course.publishedAt ? new Date(course.publishedAt) : undefined,
    priority: 0.8,
  }));

  // タグ: 公開記事に付いている全タグ + 人気タグ
  const tagLastModified = new Map<string, Date | undefined>();
  for (const article of articles) {
    for (const tag of article.tags) {
      tagLastModified.set(tag, latestDate(tagLastModified.get(tag), new Date(article.updatedAt)));
    }
  }
  for (const tag of popularTagNames) {
    if (!tagLastModified.has(tag)) tagLastModified.set(tag, undefined);
  }
  const tagRoutes: MetadataRoute.Sitemap = [...tagLastModified].map(([tag, lastModified]) => ({
    url: `${siteUrl}/tags/${encodeURIComponent(tag)}`,
    lastModified,
    changeFrequency: 'weekly',
    priority: 0.6,
  }));

  // ユーザー: 記事または講座を公開しているユーザー
  const userLastModified = new Map<string, Date | undefined>();
  for (const article of articles) {
    userLastModified.set(
      article.authorId,
      latestDate(userLastModified.get(article.authorId), new Date(article.updatedAt))
    );
  }
  for (const course of courses) {
    const current = userLastModified.get(course.userId);
    userLastModified.set(
      course.userId,
      course.publishedAt ? latestDate(current, new Date(course.publishedAt)) : current
    );
  }
  const userRoutes: MetadataRoute.Sitemap = [...userLastModified].map(([userId, lastModified]) => ({
    url: `${siteUrl}/users/${userId}`,
    lastModified,
    changeFrequency: 'weekly',
    priority: 0.5,
  }));

  return [
    ...staticRoutes,
    ...articleRoutes,
    ...courseRoutes,
    ...courseArticleRoutes,
    ...tagRoutes,
    ...userRoutes,
  ];
}
