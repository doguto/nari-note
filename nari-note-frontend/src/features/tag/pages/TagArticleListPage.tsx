import { PageWithSidebar } from '@/features/global/organisms';
import { TagArticleListTemplate } from '../templates/TagArticleListTemplate';
import type { ArticleDto } from '@/lib/api/types';

interface TagArticleListPageProps {
  tag: string;
  articles: ArticleDto[];
}

/**
 * TagArticleListPage - Page Component
 * 
 * タグ別記事一覧ページのビジネスロジックを担当するページコンポーネント
 * データはサーバーサイドで取得され、propsとして渡される
 */
export function TagArticleListPage({ tag, articles }: TagArticleListPageProps) {
  return (
    <PageWithSidebar>
      <TagArticleListTemplate tag={tag} articles={articles} />
    </PageWithSidebar>
  );
}
