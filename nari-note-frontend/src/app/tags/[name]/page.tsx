'use client';

import { useParams } from 'next/navigation';
import { Header } from '@/components/layout/Header';
import { Sidebar } from '@/components/layout/Sidebar';
import { Footer } from '@/components/layout/Footer';
import { TagArticleListContainer } from '@/features/tag/containers/TagArticleListContainer';

/**
 * タグページ
 * 
 * 特定のタグに紐づく記事一覧を表示します。
 */
export default function TagPage() {
  const params = useParams();
  const tagName = decodeURIComponent(params.name as string);

  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f5f3e8] to-[#e8e4d0]">
      <Header />
      
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="flex gap-8">
          <div className="flex-1">
            <TagArticleListContainer tag={tagName} />
          </div>
          
          <Sidebar />
        </div>
      </div>
      
      <Footer />
    </div>
  );
}
