'use client';

import { SearchBar } from '@/components/molecules';
import { ArticleCard } from '@/components/molecules';
import { EmptyState } from '@/components/ui';
import { Search } from 'lucide-react';
import { ArticleDto } from '@/lib/api/types';

interface ArticleSearchTemplateProps {
  keyword: string;
  searchKeyword: string;
  hasSearched: boolean;
  hasResults: boolean;
  articles: ArticleDto[];
  onKeywordChange: (keyword: string) => void;
  onSearch: () => void;
}

/**
 * ArticleSearchTemplate - Template Component
 * 
 * 記事検索ページのUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function ArticleSearchTemplate({
  keyword,
  searchKeyword,
  hasSearched,
  hasResults,
  articles,
  onKeywordChange,
  onSearch,
}: ArticleSearchTemplateProps) {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8 text-center" style={{ fontFamily: 'serif' }}>
        記事を探す
      </h1>
      
      <SearchBar 
        value={keyword} 
        onChange={onKeywordChange}
        onSearch={onSearch}
      />

      {/* 初期状態: まだ検索していない */}
      {!hasSearched && (
        <EmptyState
          icon={<Search />}
          title="記事を検索してください"
          description="タイトルや本文から記事を検索できます。検索キーワードを入力して「検索」ボタンを押すか、Enterキーを押してください。"
        />
      )}

      {/* 検索結果が0件 */}
      {hasSearched && !hasResults && (
        <EmptyState
          icon={<Search />}
          title="検索結果が見つかりませんでした"
          description={`「${searchKeyword}」に一致する記事が見つかりませんでした。別のキーワードで検索してみてください。`}
        />
      )}

      {/* 検索結果表示 */}
      {hasSearched && hasResults && (
        <>
          {/* 検索結果件数 */}
          <div className="mb-4">
            <p className="text-gray-600">
              {articles.length}件の記事が見つかりました
            </p>
          </div>

          {/* 検索結果一覧 */}
          <div className="space-y-4">
            {articles.map((article) => (
              <ArticleCard
                key={article.id}
                id={article.id!}
                title={article.title ?? ''}
                author={article.authorName ?? ''}
                authorId={article.authorId ?? 0}
                likeCount={article.likeCount ?? 0}
                date={article.publishedAt 
                  ? new Date(article.publishedAt).toLocaleDateString('ja-JP') 
                  : article.createdAt 
                    ? new Date(article.createdAt).toLocaleDateString('ja-JP') 
                    : ''
                }
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
