import { PageWithSidebar } from '@/features/global/organisms';
import { TagArticleListTemplate } from '../templates/TagArticleListTemplate';
import { getArticlesByTag } from '@/lib/api/server';

interface TagArticleListPageProps {
  tagName: string;
}

/**
 * TagArticleListPage - Page Component
 * 
 * タグ別記事一覧ページのビジネスロジックを担当するページコンポーネント
 * サーバーサイドでデータフェッチを行い、Templateに渡す
 */
export async function TagArticleListPage({ tagName }: TagArticleListPageProps) {
  // URLエンコードされたタグ名をデコード
  const decodedTagName = decodeURIComponent(tagName);
  
  // サーバーサイドでデータをフェッチ
  const data = await getArticlesByTag({ tagName: decodedTagName }).catch(() => ({ articles: [] }));
  const articles = data?.articles ?? [];

  return (
    <PageWithSidebar>
      <TagArticleListTemplate tag={decodedTagName} articles={articles} />
    </PageWithSidebar>
  );
}
