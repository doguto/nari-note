'use client';

import { useParams } from 'next/navigation';
import { Header } from '@/components/layout/Header';
import { Sidebar } from '@/components/layout/Sidebar';
import { Footer } from '@/components/layout/Footer';
import { ArticleDetailPage } from '@/features/article/organisms';

export default function ArticleDetailPageRoute() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <div className="min-h-screen bg-gradient-to-b from-brand-bg-gradient-from to-brand-bg-gradient-to flex flex-col">
      <Header />
      
      <div className="flex-1 max-w-7xl mx-auto px-4 py-8 w-full">
        <div className="flex gap-8">
          <div className="flex-1">
            <ArticleDetailPage articleId={articleId} />
          </div>
          
          <Sidebar />
        </div>
      </div>
      
      <Footer />
    </div>
  );
}
