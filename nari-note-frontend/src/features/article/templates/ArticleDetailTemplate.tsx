'use client';

import Link from 'next/link';
import ReactMarkdown from 'react-markdown';
import { LikeButton } from '@/components/ui';
import { CommentForm } from '../organisms/CommentForm';
import { CommentList } from '../organisms/CommentList';
import { Comment } from '@/types/comment';
import { Button } from '@/components/ui/button';
<<<<<<< HEAD
import { Pencil } from 'lucide-react';
=======
import { Pencil, BookOpen, ChevronRight } from 'lucide-react';
>>>>>>> 6295498e9d945624c7feac83fa134d07af2b57ac
import { GetArticleContentResponse } from '@/lib/api/types';

interface ArticleDetailTemplateProps {
  article: GetArticleContentResponse;
  comments: Comment[];
  isOwnArticle: boolean;
  isLikePending: boolean;
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
  article,
  comments,
  isOwnArticle,
  isLikePending,
  onLikeClick,
  onCommentSuccess,
}: ArticleDetailTemplateProps) {
  return (
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
          <div className="w-12 h-12 bg-brand-primary rounded-full flex items-center justify-center text-white font-bold">
            {article.authorName?.charAt(0).toUpperCase() || 'A'}
          </div>
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
        <ReactMarkdown
          components={{
            h1: ({ ...props }) => <h1 className="text-3xl font-bold mt-8 mb-4" {...props} />,
            h2: ({ ...props }) => <h2 className="text-2xl font-bold mt-6 mb-3" {...props} />,
            h3: ({ ...props }) => <h3 className="text-xl font-bold mt-4 mb-2" {...props} />,
            p: ({ ...props }) => <p className="mb-4" {...props} />,
            code: ({ className, children, ...props }) => {
              const isInline = !className;
              return isInline ? (
                <code className="bg-gray-100 px-1.5 py-0.5 rounded text-sm font-mono text-pink-600" {...props}>
                  {children}
                </code>
              ) : (
                <code className="block bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto font-mono text-sm" {...props}>
                  {children}
                </code>
              );
            },
            pre: ({ ...props }) => <pre className="my-4" {...props} />,
            ul: ({ ...props }) => <ul className="list-disc list-inside mb-4 space-y-1" {...props} />,
            ol: ({ ...props }) => <ol className="list-decimal list-inside mb-4 space-y-1" {...props} />,
            blockquote: ({ ...props }) => <blockquote className="border-l-4 border-gray-300 pl-4 italic my-4" {...props} />,
            a: ({ ...props }) => <a className="text-brand-primary hover:underline" {...props} />,
          }}
        >
          {article.body}
        </ReactMarkdown>
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
  );
}
