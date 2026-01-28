import { AuthGuard } from '@/components/molecules';
import { ArticleFormPage } from '@/features/article/pages';

export default function NewArticlePage() {
  return(
    <AuthGuard redirectPath='/articles/new'>
      <ArticleFormPage />
    </AuthGuard>
  );
}
