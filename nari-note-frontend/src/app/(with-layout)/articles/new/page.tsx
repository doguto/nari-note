import type { Metadata } from 'next';
import { AuthGuard } from '@/features/global/organisms';
import { ArticleFormPage } from '@/features/article/pages';

export const metadata: Metadata = {
  robots: {
    index: false,
    follow: false,
  },
};

export default function NewArticlePage() {
  return(
    <AuthGuard redirectPath='/articles/new'>
      <ArticleFormPage />
    </AuthGuard>
  );
}
