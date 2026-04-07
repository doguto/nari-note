'use client';

import { useState } from 'react';
import { useSearchArticles } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { ArticleSearchTemplate } from '../templates/ArticleSearchTemplate';

/**
 * ArticleSearchPage - Page Component
 * 
 * 記事検索ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 */
export function ArticleSearchPage() {
  const [keyword, setKeyword] = useState('');
  const [searchKeyword, setSearchKeyword] = useState('');

  // 検索実行: 検索ボタンクリックまたはEnterキーで実行
  const handleSearch = () => {
    if (keyword.trim()) {
      setSearchKeyword(keyword.trim());
    }
  };

  // キーワードが空の場合は検索を無効化
  const { data, isLoading, error, refetch } = useSearchArticles(
    { keyword: searchKeyword, limit: 20, offset: 0 },
    { enabled: searchKeyword.length > 0 }
  );

  // 検索結果表示の判定
  const hasSearched = searchKeyword.length > 0;
  const hasResults = !!(data?.articles && data.articles.length > 0);

  // IDが存在しない記事をフィルタリング
  const articlesWithId = (data?.articles || []).filter((article) => article.id !== null && article.id !== undefined);

  if (hasSearched && isLoading) {
    return <LoadingSpinner text="検索中..." />;
  }

  if (hasSearched && error) {
    return (
      <ErrorMessage 
        message="記事の検索に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  return (
    <ArticleSearchTemplate
      keyword={keyword}
      searchKeyword={searchKeyword}
      hasSearched={hasSearched}
      hasResults={hasResults}
      articles={articlesWithId}
      onKeywordChange={setKeyword}
      onSearch={handleSearch}
    />
  );
}
