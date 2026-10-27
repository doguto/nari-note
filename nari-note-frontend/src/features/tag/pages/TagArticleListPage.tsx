import { PageWithSidebar } from '@/features/global/organisms';
import { TagArticleListTemplate } from '../templates/TagArticleListTemplate';
import { getArticlesByTag } from '@/lib/api/server';

const PAGE_SIZE = 20;

interface TagArticleListPageProps {
  tagName: string;
  page: number;
}

/**
 * TagArticleListPage - Page Component
 * 
 * タグ別記事一覧ページのビジネスロジックを担当するページコンポーネント
 * サーバーサイドでデータフェッチを行い、Templateに渡す
 */
export async function TagArticleListPage({ tagName, page }: TagArticleListPageProps) {
  // URLエンコードされたタグ名をデコード
  const decodedTagName = decodeURIComponent(tagName);
  
  // サーバーサイドでデータをフェッチ
  const data = await getArticlesByTag({
    tagName: decodedTagName,
    limit: PAGE_SIZE,
    offset: (page - 1) * PAGE_SIZE,
  }).catch(() => ({ articles: [], totalCount: 0 }));
  const articles = data?.articles ?? [];
  const totalCount = data?.totalCount ?? 0;

  return (
    <PageWithSidebar>
      <TagArticleListTemplate
        tag={decodedTagName}
        articles={articles}
        totalCount={totalCount}
        currentPage={page}
        pageSize={PAGE_SIZE}
      />
    </PageWithSidebar>
  );
}
