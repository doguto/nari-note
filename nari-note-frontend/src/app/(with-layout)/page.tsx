import { HomeListPage } from "@/features/home/pages";
import { getArticles, getCourses } from "@/lib/api/server";

export default async function Home() {
  // サーバーサイドでデータをフェッチ
  const [articlesData, coursesData] = await Promise.all([
    getArticles({ limit: 20, offset: 0 }).catch(() => ({ articles: [] })),
    getCourses({ limit: 20, offset: 0 }).catch(() => ({ courses: [] })),
  ]);

  // IDが存在する記事と講座のみをフィルタリング
  const articlesWithId = articlesData?.articles?.filter(
    (article) => article.id !== null && article.id !== undefined
  ) || [];
  
  const coursesWithId = coursesData?.courses?.filter(
    (course) => course.id !== null && course.id !== undefined
  ) || [];

  return (
    <HomeListPage 
      articles={articlesWithId}
      courses={coursesWithId}
    />
  );
}
