'use client';

import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { ArticleFormContainer } from '@/features/article/containers/ArticleFormContainer';

/**
 * 新規記事作成ページ
 * 
 * ユーザーが新しい記事を作成するためのページ。
 * マークダウン形式で記事を執筆でき、プレビュー機能も提供します。
 */
export default function NewArticlePage() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f5f3e8] to-[#e8e4d0]">
      <Header />
      
      <div className="max-w-5xl mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-[#2d3e1f] mb-2">
            新規記事作成
          </h1>
          <p className="text-gray-600">
            マークダウン形式で記事を作成できます。プレビュー機能を使用して、公開前に記事の見た目を確認できます。
          </p>
        </div>
        
        <div className="bg-white rounded-lg shadow-lg p-8">
          <ArticleFormContainer />
        </div>
      </div>
      
      <Footer />
    </div>
  );
}
