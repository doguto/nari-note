import { FormPageLayout } from '@/components/molecules';
import { ArticleFormPage } from '@/features/article/pages';

export default function NewArticlePage() {
  return (
    <FormPageLayout 
      title="新規記事作成"
      description="マークダウン形式で記事を作成できます。プレビュー機能を使用して、公開前に記事の見た目を確認できます。"
    >
      <ArticleFormPage />
    </FormPageLayout>
  );
}
