'use client';

import { useState } from 'react';
import { useSearchArticles } from '@/lib/api';
import { SearchBar } from '@/components/common/molecules';
import { ArticleCard } from '@/components/common/molecules';
import { LoadingSpinner, ErrorMessage, EmptyState } from '@/components/common/atoms';
import { Search } from 'lucide-react';

/**
 * ArticleSearchPage - Organism Component
 * 
 * 記事検索ページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
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
  const hasResults = data?.articles && data.articles.length > 0;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
        記事を探す
      </h1>
      
      <SearchBar 
        value={keyword} 
        onChange={setKeyword}
        onSearch={handleSearch}
      />

      {/* 初期状態: まだ検索していない */}
      {!hasSearched && (
        <EmptyState
          icon={<Search />}
          title="記事を検索してください"
          description="タイトルや本文から記事を検索できます。検索キーワードを入力して「検索」ボタンを押すか、Enterキーを押してください。"
        />
      )}

      {/* ローディング中 */}
      {hasSearched && isLoading && (
        <LoadingSpinner text="検索中..." />
      )}

      {/* エラー */}
      {hasSearched && error && (
        <ErrorMessage 
          message="記事の検索に失敗しました" 
          onRetry={refetch}
        />
      )}

      {/* 検索結果が0件 */}
      {hasSearched && !isLoading && !error && !hasResults && (
        <EmptyState
          icon={<Search />}
          title="検索結果が見つかりませんでした"
          description={`「${searchKeyword}」に一致する記事が見つかりませんでした。別のキーワードで検索してみてください。`}
        />
      )}

      {/* 検索結果表示 */}
      {hasSearched && !isLoading && !error && hasResults && (
        <>
          {/* 検索結果件数 */}
          <div className="mb-4">
            <p className="text-gray-600">
              {(data.articles || []).filter((article) => article.id != null).length}件の記事が見つかりました
            </p>
          </div>

          {/* 検索結果一覧 */}
          {/* APIレスポンスの型定義上、IDが存在しない可能性があるためフィルタリング */}
          <div className="space-y-4">
            {(data.articles || [])
              .filter((article) => article.id != null)
              .map((article) => (
                <ArticleCard
                  key={article.id}
                  id={article.id as number}
                  title={article.title ?? ''}
                  author={article.authorName ?? ''}
                  authorId={article.authorId ?? 0}
                  stats={`いいね ${article.likeCount ?? 0}`}
                  date={article.publishedAt 
                    ? new Date(article.publishedAt).toLocaleDateString('ja-JP') 
                    : article.createdAt 
                      ? new Date(article.createdAt).toLocaleDateString('ja-JP') 
                      : ''
                  }
                  image="📝"
                />
              ))}
          </div>
        </>
      )}
    </div>
  );
}
