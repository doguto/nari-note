import { AuthGuard } from '@/features/global/organisms';
import { ArticleFormPage } from '@/features/article/pages';

export default function NewArticlePage() {
  return(
    <AuthGuard redirectPath='/articles/new'>
      <ArticleFormPage />
    </AuthGuard>
  );
}
