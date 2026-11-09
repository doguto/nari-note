import { PageWithSidebar } from '@/features/global/organisms';
import { TagArticleListTemplate, type TagTab } from '../templates/TagArticleListTemplate';
import { getArticlesByTag, getCoursesByTag } from '@/lib/api/server';

const PAGE_SIZE = 20;

interface TagArticleListPageProps {
  tagName: string;
  activeTab: TagTab;
  page: number;
}

/**
 * TagArticleListPage - Page Component
 *
 * タグ別一覧ページ（記事・講座）のビジネスロジックを担当するページコンポーネント
 * サーバーサイドで表示中のタブのデータだけをフェッチし、Templateに渡す
 */
export async function TagArticleListPage({ tagName, activeTab, page }: TagArticleListPageProps) {
  // URLエンコードされたタグ名をデコード
  const decodedTagName = decodeURIComponent(tagName);
  const params = {
    tagName: decodedTagName,
    limit: PAGE_SIZE,
    offset: (page - 1) * PAGE_SIZE,
  };

  const articleData = activeTab === 'articles'
    ? await getArticlesByTag(params).catch(() => ({ articles: [], totalCount: 0 }))
    : undefined;
  const courseData = activeTab === 'courses'
    ? await getCoursesByTag(params).catch(() => ({ courses: [], totalCount: 0 }))
    : undefined;

  return (
    <PageWithSidebar>
      <TagArticleListTemplate
        tag={decodedTagName}
        activeTab={activeTab}
        articles={articleData?.articles ?? []}
        courses={courseData?.courses ?? []}
        totalCount={(activeTab === 'articles' ? articleData?.totalCount : courseData?.totalCount) ?? 0}
        currentPage={page}
        pageSize={PAGE_SIZE}
      />
    </PageWithSidebar>
  );
}
