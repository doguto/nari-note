'use client';

import { useState, useEffect } from 'react';
import { useSearchParams } from 'next/navigation';
import { Input } from '@/components/ui/input';
import { ArticleCard } from '@/components/common/ArticleCard';
import { Pagination } from '@/components/common/Pagination';
import { Badge } from '@/components/ui/badge';
import { mockArticles, mockTags } from '@/lib/mockData';
import { Search as SearchIcon } from 'lucide-react';

export default function SearchPage() {
  const searchParams = useSearchParams();
  const initialQuery = searchParams.get('q') || '';
  
  const [searchQuery, setSearchQuery] = useState(initialQuery);
  const [debouncedQuery, setDebouncedQuery] = useState(initialQuery);
  const [currentPage, setCurrentPage] = useState(1);
  const articlesPerPage = 20;

  // デバウンス処理
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(searchQuery);
      setCurrentPage(1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchQuery]);

  // 検索結果のフィルタリング
  const filteredArticles = mockArticles.filter(article => {
    if (!debouncedQuery.trim()) return false;
    
    const query = debouncedQuery.toLowerCase();
    return (
      article.title.toLowerCase().includes(query) ||
      article.content.toLowerCase().includes(query) ||
      article.tags.some(tag => tag.name.toLowerCase().includes(query))
    );
  });

  const totalPages = Math.ceil(filteredArticles.length / articlesPerPage);
  const indexOfLastArticle = currentPage * articlesPerPage;
  const indexOfFirstArticle = indexOfLastArticle - articlesPerPage;
  const currentArticles = filteredArticles.slice(indexOfFirstArticle, indexOfLastArticle);

  // 人気タグ（サジェスト用）
  const popularTags = mockTags.slice(0, 6);

  const handleTagClick = (tagName: string) => {
    setSearchQuery(tagName);
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold mb-6">記事を検索</h1>

        {/* 検索バー */}
        <div className="mb-8">
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <Input
              type="search"
              placeholder="キーワードを入力して検索..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 text-lg"
              autoFocus
            />
          </div>

          {/* 人気タグ（サジェスト） */}
          {!debouncedQuery && (
            <div className="mt-4">
              <p className="text-sm text-gray-600 mb-2">人気のタグ:</p>
              <div className="flex flex-wrap gap-2">
                {popularTags.map((tag) => (
                  <Badge
                    key={tag.id}
                    variant="outline"
                    className="cursor-pointer hover:bg-gray-100"
                    onClick={() => handleTagClick(tag.name)}
                  >
                    #{tag.name}
                  </Badge>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* 検索結果 */}
        {debouncedQuery ? (
          <>
            <div className="mb-6">
              <h2 className="text-xl font-semibold">
                「{debouncedQuery}」の検索結果: {filteredArticles.length}件
              </h2>
            </div>

            {filteredArticles.length === 0 ? (
              <div className="text-center py-12">
                <p className="text-gray-500 mb-2">検索条件に一致する記事が見つかりませんでした。</p>
                <p className="text-sm text-gray-400">別のキーワードで検索してみてください。</p>
              </div>
            ) : (
              <>
                <div className="grid gap-6 mb-8">
                  {currentArticles.map((article) => (
                    <ArticleCard key={article.id} article={article} />
                  ))}
                </div>

                {totalPages > 1 && (
                  <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={setCurrentPage}
                  />
                )}
              </>
            )}
          </>
        ) : (
          <div className="text-center py-12 text-gray-500">
            <SearchIcon className="w-16 h-16 mx-auto mb-4 text-gray-300" />
            <p>キーワードを入力して記事を検索してください</p>
          </div>
        )}
      </div>
    </div>
  );
}
