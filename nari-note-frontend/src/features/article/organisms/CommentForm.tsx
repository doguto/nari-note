'use client';

import { useState } from 'react';
import { CommentField } from './CommentField';
import { ErrorAlert } from '@/components/ui/ErrorAlert';
import { Button } from '@/components/ui/button';
import { useCreateComment } from '@/lib/api';
import { useRequireVerifiedEmail } from '@/lib/hooks';
import { useAuth } from '@/lib/providers/AuthProvider';
import { CharacterCounter, UserAvatar } from '@/components/ui';

const MAX_LENGTH = 1000;

interface CommentFormProps {
  articleId: string;
  onSuccess?: () => void;
}

/**
 * CommentForm - Organism Component
 * 
 * 記事へのコメント投稿フォーム
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function CommentForm({ articleId, onSuccess }: CommentFormProps) {
  const [message, setMessage] = useState('');
  const [error, setError] = useState<string>('');
  const { ensureVerifiedEmail } = useRequireVerifiedEmail();
  const { userName, userIconImageUrl } = useAuth();

  const createComment = useCreateComment({
    onSuccess: () => {
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

    if (!ensureVerifiedEmail()) return;


    // バリデーション
    if (!message.trim()) {
      setError('コメントを入力してください');
      return;
    }

    if (message.length > MAX_LENGTH) {
      setError('コメントは1000文字以内で入力してください');
      return;
    }

    setError('');
    createComment.mutate({ articleId, message });
  };

  return (
    <div className="flex items-start gap-3">
      <UserAvatar username={userName || 'ゲスト'} userIconImageUrl={userIconImageUrl ?? undefined} size="sm" />
      <div className="min-w-0 flex-1 space-y-2">
        {createComment.error && (
          <ErrorAlert message="コメントの投稿に失敗しました" />
        )}

        {createComment.isSuccess && (
          <p className="text-sm text-green-600">コメントを投稿しました</p>
        )}

        <form onSubmit={handleSubmit} className="space-y-2">
          <CommentField
            value={message}
            onChange={setMessage}
            error={error}
            maxLength={MAX_LENGTH}
          />
          <div className="flex items-center justify-between gap-2">
            <CharacterCounter count={message.length} max={MAX_LENGTH} />
            <Button
              type="submit"
              size="sm"
              disabled={createComment.isPending}
              className="bg-brand-primary hover:bg-brand-primary-hover text-white"
            >
              {createComment.isPending ? '投稿中...' : '投稿'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
