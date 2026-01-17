'use client';

import { useState } from 'react';
import { CommentField } from '@/components/common/molecules/CommentField';
import { ErrorAlert } from '@/components/common/atoms/ErrorAlert';
import { Button } from '@/components/ui/button';
import { useCreateComment } from '@/lib/api';
import { Comment } from '@/types/comment';

interface CommentFormProps {
  articleId: number;
  onCommentAdded?: (comment: Comment) => void;
  onSuccess?: () => void;
}

/**
 * CommentForm - Organism Component
 * 
 * 記事へのコメント投稿フォーム
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function CommentForm({ articleId, onCommentAdded, onSuccess }: CommentFormProps) {
  const [message, setMessage] = useState('');
  const [error, setError] = useState<string>('');

  const createComment = useCreateComment({
    onSuccess: (data) => {
      // 投稿したコメントを作成（仮のユーザー情報を使用）
      // TODO: 実際のユーザー情報をAPIから取得または認証コンテキストから取得
      const newComment: Comment = {
        id: data.id || Date.now(),
        userId: 0, // 仮の値
        userName: '現在のユーザー', // 仮の値
        message: message,
        createdAt: data.createdAt || new Date().toISOString(),
      };
      
      onCommentAdded?.(newComment);
      setMessage('');
      setError('');
      onSuccess?.();
    },
    onError: (err) => {
      console.error('コメント投稿エラー:', err);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // バリデーション
    if (!message.trim()) {
      setError('コメントを入力してください');
      return;
    }

    if (message.length > 1000) {
      setError('コメントは1000文字以内で入力してください');
      return;
    }

    setError('');
    createComment.mutate({ articleId, message });
  };

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <h2 className="text-xl font-bold text-brand-text mb-4">コメントを投稿</h2>
      
      {createComment.error && (
        <ErrorAlert message="コメントの投稿に失敗しました" />
      )}

      {createComment.isSuccess && (
        <div className="bg-green-50 border border-green-200 rounded p-3 mb-4">
          <p className="text-green-600 text-sm">コメントを投稿しました</p>
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <CommentField 
          value={message} 
          onChange={setMessage}
          error={error}
        />
        
        <div className="flex justify-end">
          <Button 
            type="submit" 
            disabled={createComment.isPending}
            className="bg-brand-primary hover:bg-brand-primary-hover text-white"
          >
            {createComment.isPending ? '投稿中...' : 'コメントを投稿'}
          </Button>
        </div>
      </form>
    </div>
  );
}
