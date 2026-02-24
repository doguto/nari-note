import Link from 'next/link';
import { LikeButton, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { NarinoteMarkdown, CourseBreadcrumb, ArticleAuthorInfo, ArticleTagList } from '@/components/molecules';
import { CommentForm } from '../organisms/CommentForm';
import { CommentList } from '../organisms/CommentList';
import { Comment } from '@/types/comment';
import { Button } from '@/components/ui/button';
import { Pencil } from 'lucide-react';
import { GetArticleContentResponse } from '@/lib/api/types';
import { PageWithSidebar } from '@/features/global/organisms';

interface ArticleDetailTemplateProps {
  isLoading: boolean;
  error: Error | null;
  article: GetArticleContentResponse | undefined;
  comments: Comment[];
  isOwnArticle: boolean;
  isLikePending: boolean;
  onRetry: () => void;
  onLikeClick: () => void;
  onCommentSuccess: () => void;
}

/**
 * ArticleDetailTemplate - Template Component
 *
 * 記事詳細ページのUI構成とレイアウトを担当
 * Organism/Moleculeを組み合わせてレスポンシブなUIを構築
 */
export function ArticleDetailTemplate({
  isLoading,
  error,
  article,
  comments,
  isOwnArticle,
  isLikePending,
  onRetry,
  onLikeClick,
  onCommentSuccess,
}: ArticleDetailTemplateProps) {
  if (isLoading) {
    return (
      <PageWithSidebar>
        <LoadingSpinner text="記事を読み込み中..." />
      </PageWithSidebar>
    );
  }

  if (error) {
    return (
      <PageWithSidebar>
        <ErrorMessage message="記事の取得に失敗しました" onRetry={onRetry} />
      </PageWithSidebar>
    );
  }

  if (!article) {
    return (
      <PageWithSidebar>
        <ErrorMessage message="記事が見つかりません" />
      </PageWithSidebar>
    );
  }

  return (
    <PageWithSidebar>
    <article className="bg-white rounded-lg shadow-lg p-8">
      {article.courseId && article.courseName && (
        <CourseBreadcrumb courseId={article.courseId} courseName={article.courseName} />
      )}

      <div className="flex items-center justify-between mb-6">
        <h1 className="text-4xl font-bold text-brand-text">
          {article.title}
        </h1>
        {isOwnArticle && (
          <Link href={`/articles/${article.id}/edit`}>
            <Button
              variant="outline"
              className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
            >
              <Pencil className="w-4 h-4 mr-2" />
              編集
            </Button>
          </Link>
        )}
      </div>

      <div className="flex items-center gap-6 mb-8 pb-6 border-b border-gray-200">
        <ArticleAuthorInfo
          authorId={article.authorId!}
          authorName={article.authorName || 'Unknown Author'}
          createdAt={article.createdAt}
        />

        <div className="flex items-center gap-4 ml-auto">
          <LikeButton
            isLiked={article.isLiked || false}
            likeCount={article.likeCount || 0}
            onClick={onLikeClick}
            disabled={isLikePending}
          />
          <button className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 transition-colors">
            ストック
          </button>
        </div>
      </div>

      <div className="prose prose-lg max-w-none mb-8 text-gray-800 leading-relaxed">
        <NarinoteMarkdown content={article.body || ''} />
      </div>

      {article.tags && article.tags.length > 0 && (
        <ArticleTagList tags={article.tags} />
      )}

      {/* コメント一覧 */}
      <div className="mt-8">
        <CommentList comments={comments} />
      </div>

      {/* コメント投稿フォーム */}
      <div className="mt-8">
        <CommentForm articleId={article.id!} onSuccess={onCommentSuccess} />
      </div>
    </article>
    </PageWithSidebar>
  );
}
