import { UnifiedSearchPage as UnifiedSearchPageComponent } from '@/features/search/pages';

interface SearchPageProps {
  searchParams: Promise<{ q?: string; tab?: string; page?: string }>;
}

export default async function UnifiedSearchPage({ searchParams }: SearchPageProps) {
  const { q, tab, page } = await searchParams;
  const searchKeyword = q?.trim() ?? '';
  const activeTab = tab === 'courses' ? 'courses' : 'articles';
  const parsedPage = Number(page);
  const currentPage = Number.isInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1;

  // キーワードが変わったら入力欄の状態を作り直す
  return (
    <UnifiedSearchPageComponent
      key={searchKeyword}
      searchKeyword={searchKeyword}
      activeTab={activeTab}
      page={currentPage}
    />
  );
}
