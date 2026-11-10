import Link from 'next/link';
import { LikeButton, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { NarinoteMarkdown, CourseBreadcrumb, ArticleAuthorInfo, ArticleTagList, ShareButtons } from '@/components/molecules';
import { CommentForm } from '../organisms/CommentForm';
import { CommentList } from '../organisms/CommentList';
import { Comment } from '@/types/comment';
import { Pencil, X } from 'lucide-react';
import { GetArticleContentResponse } from '@/lib/api/types';

interface ArticleDetailTemplateProps {
  isLoading: boolean;
  error: Error | null;
  article: GetArticleContentResponse | undefined;
  comments: Comment[];
  isOwnArticle: boolean;
  isLikePending: boolean;
  showPublishedBanner: boolean;
  onDismissPublishedBanner: () => void;
  onRetry: () => void;
  onLikeClick: () => void;
  onCommentSuccess: () => void;
}

export function ArticleDetailTemplate({
  isLoading,
  error,
  article,
  comments,
  isOwnArticle,
  isLikePending,
  showPublishedBanner,
  onDismissPublishedBanner,
  onRetry,
  onLikeClick,
  onCommentSuccess,
}: ArticleDetailTemplateProps) {
  if (isLoading) {
    return (
      <LoadingSpinner text="記事を読み込み中..." />
    );
  }

  if (error) {
    return (
      <ErrorMessage message="記事の取得に失敗しました" onRetry={onRetry} />
    );
  }

  if (!article) {
    return (
      <ErrorMessage message="記事が見つかりません" />
    );
  }

  const articlePath = `/articles/${article.article.id}`;

  return (
    <article className="bg-white rounded-lg shadow-lg p-4 md:p-8">
      {showPublishedBanner && (
        <div className="relative mb-6 rounded-lg border border-green-200 bg-green-50 p-4 pr-10" role="status">
          <p className="mb-3 font-medium text-green-800">
            記事を公開しました！X でシェアして、たくさんの人に読んでもらいましょう
          </p>
          <ShareButtons title={article.article.title} path={articlePath} tags={article.article.tags} />
          <button
            type="button"
            onClick={onDismissPublishedBanner}
            className="absolute right-3 top-3 text-green-700 hover:text-green-900"
            aria-label="閉じる"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      )}

      {(isOwnArticle || (article.courseId && article.courseName)) && (
        <div className="mb-2 flex items-center gap-2">
          {article.courseId && article.courseName && (
            <CourseBreadcrumb courseId={article.courseId} courseName={article.courseName} />
          )}
          {isOwnArticle && (
            <Link
              href={`/articles/${article.article.id}/edit`}
              className="ml-auto inline-flex shrink-0 items-center gap-1 text-sm font-semibold text-brand-primary-dark underline underline-offset-4 hover:text-brand-primary"
            >
              <Pencil className="h-3.5 w-3.5" />
              編集
            </Link>
          )}
        </div>
      )}

      <h1 className="mb-4 md:mb-6 break-words text-2xl md:text-4xl font-bold text-brand-text">
        {article.article.title}
      </h1>

      <div className="flex items-center gap-3 md:gap-6 mb-4 md:mb-8 pb-3 md:pb-6 border-b border-gray-200">
        <ArticleAuthorInfo
          authorId={article.article.authorId}
          authorName={article.article.authorName}
          userIconImageUrl={article.article.userIconImageUrl}
          createdAt={article.article.createdAt}
        />

        <div className="flex items-center gap-4 ml-auto">
          <LikeButton
            isLiked={article.isLiked || false}
            likeCount={article.article.likeCount}
            onClick={onLikeClick}
            disabled={isLikePending}
          />
        </div>
      </div>

      <div className="prose prose-base md:prose-lg max-w-none mb-8 text-gray-800 leading-relaxed">
        <NarinoteMarkdown content={article.article.body} kifuList={article.article.kifus} />
      </div>

      {article.article.tags && article.article.tags.length > 0 && (
        <ArticleTagList tags={article.article.tags} />
      )}

      <div className="mt-6">
        <ShareButtons title={article.article.title} path={articlePath} tags={article.article.tags} />
      </div>

      <section className="mt-8 border-t border-gray-200 pt-6 space-y-6">
        <CommentList comments={comments} />
        <CommentForm articleId={article.article.id} onSuccess={onCommentSuccess} />
      </section>
    </article>
  );
}
