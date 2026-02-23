import Link from 'next/link';
import { LikeButton, UserAvatar, LoadingSpinner, ErrorMessage } from '@/components/ui';
import { NarinoteMarkdown } from '@/components/molecules';
import { CommentForm } from '../organisms/CommentForm';
import { CommentList } from '../organisms/CommentList';
import { Comment } from '@/types/comment';
import { Button } from '@/components/ui/button';
import { Pencil, BookOpen, ChevronRight } from 'lucide-react';
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
      {/* Course breadcrumb - only show if article is part of a course */}
      {article.courseId && article.courseName && (
        <div className="mb-4 flex items-center gap-2 text-sm text-gray-600">
          <BookOpen className="w-4 h-4" />
          <Link
            href={`/courses/${article.courseId}`}
            className="hover:text-brand-primary hover:underline flex items-center gap-1"
          >
            {article.courseName}
          </Link>
          <ChevronRight className="w-4 h-4" />
          <span className="text-gray-800 font-medium">この記事</span>
        </div>
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
        <Link href={`/users/${article.authorId}`} className="flex items-center gap-3 hover:opacity-80 transition-opacity">
          <UserAvatar username={article.authorName || 'Unknown Author'} size="md" />
          <div>
            <div className="font-medium text-brand-text">
              {article.authorName || 'Unknown Author'}
            </div>
            <div className="text-sm text-gray-500">
              {article.createdAt ? new Date(article.createdAt).toLocaleDateString('ja-JP') : ''}
            </div>
          </div>
        </Link>
        
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
        <div className="flex gap-2 flex-wrap pt-6 border-t border-gray-200">
          {article.tags.map((tag) => (
            <Link
              key={tag}
              href={`/tags/${tag}`}
              className="px-3 py-1 bg-brand-bg-light text-brand-text rounded-full text-sm hover:bg-brand-bg-gradient-to transition-colors"
            >
              #{tag}
            </Link>
          ))}
        </div>
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
