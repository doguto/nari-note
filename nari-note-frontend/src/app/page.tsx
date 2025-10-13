'use client';

import { useState } from 'react';
import { ArticleCard } from '@/components/common/ArticleCard';
import { Pagination } from '@/components/common/Pagination';
import { mockArticles } from '@/lib/mockData';

export default function Home() {
  const [currentPage, setCurrentPage] = useState(1);
  const articlesPerPage = 20;
  const totalPages = Math.ceil(mockArticles.length / articlesPerPage);

  const indexOfLastArticle = currentPage * articlesPerPage;
  const indexOfFirstArticle = indexOfLastArticle - articlesPerPage;
  const currentArticles = mockArticles.slice(indexOfFirstArticle, indexOfLastArticle);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8 p-6 rounded-lg" style={{ background: 'linear-gradient(135deg, #faf8f5 0%, #f5f1ea 100%)', borderLeft: '4px solid var(--wa-red)' }}>
        <h1 className="text-3xl font-bold mb-2" style={{ color: '#c41e3a' }}>タイムライン</h1>
        <p className="text-gray-600">最新の将棋記事をチェックしよう</p>
      </div>

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
    </div>
  );
}
