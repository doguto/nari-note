'use client';

import { useState, useEffect } from 'react';
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
  const [debouncedKeyword, setDebouncedKeyword] = useState('');

  // デバウンス処理: ユーザーの入力が止まってから300ms後に検索実行
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedKeyword(keyword);
    }, 300);

    return () => clearTimeout(timer);
  }, [keyword]);

  // キーワードが空の場合は検索を無効化
  const { data, isLoading, error, refetch } = useSearchArticles(
    { keyword: debouncedKeyword, limit: 20, offset: 0 },
    { enabled: debouncedKeyword.length > 0 }
  );

  // 初期状態: キーワードが入力されていない
  if (!keyword) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
          記事を探す
        </h1>
        <SearchBar 
          value={keyword} 
          onChange={setKeyword}
        />
        <EmptyState
          icon={<Search />}
          title="記事を検索してください"
          description="タイトルや本文から記事を検索できます。検索キーワードを入力してください。"
        />
      </div>
    );
  }

  // ローディング中
  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
          記事を探す
        </h1>
        <SearchBar 
          value={keyword} 
          onChange={setKeyword}
        />
        <LoadingSpinner text="検索中..." />
      </div>
    );
  }

  // エラー
  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
          記事を探す
        </h1>
        <SearchBar 
          value={keyword} 
          onChange={setKeyword}
        />
        <ErrorMessage 
          message="記事の検索に失敗しました" 
          onRetry={refetch}
        />
      </div>
    );
  }

  // 検索結果が0件
  if (!data?.articles || data.articles.length === 0) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
          記事を探す
        </h1>
        <SearchBar 
          value={keyword} 
          onChange={setKeyword}
        />
        <EmptyState
          icon="🔍"
          title="検索結果が見つかりませんでした"
          description={`「${keyword}」に一致する記事が見つかりませんでした。別のキーワードで検索してみてください。`}
        />
      </div>
    );
  }

  // 検索結果表示
  const articlesWithId = data.articles.filter((article) => article.id != null);

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
        記事を探す
      </h1>
      <SearchBar 
        value={keyword} 
        onChange={setKeyword}
      />
      
      {/* 検索結果件数 */}
      <div className="mb-4">
        <p className="text-gray-600">
          {articlesWithId.length}件の記事が見つかりました
        </p>
      </div>

      {/* 検索結果一覧 */}
      <div className="space-y-4">
        {articlesWithId.map((article) => (
          <ArticleCard
            key={article.id}
            id={article.id!}
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
    </div>
  );
}
