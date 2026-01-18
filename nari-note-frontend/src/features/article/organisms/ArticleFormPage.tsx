'use client';

import dynamic from 'next/dynamic';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import ReactMarkdown from 'react-markdown';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { TagInput, CharacterCounter } from '@/components/common/molecules';
import { useCreateArticle, useUpdateArticle, useGetArticle } from '@/lib/api';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PublishSettingsDialog } from './PublishSettingsDialog';

const MDEditor = dynamic(
  () => import('@uiw/react-md-editor').then((mod) => mod.default),
  { 
    ssr: false,
    loading: () => (
      <div className="flex items-center justify-center h-[400px] border border-gray-300 rounded-lg bg-gray-50">
        <div className="text-gray-500">エディターを読み込み中...</div>
      </div>
    )
  }
);

interface ArticleFormPageProps {
  articleId?: number;
  mode?: 'create' | 'edit';
}

/**
 * ArticleFormPage - Organism Component
 * 
 * 記事作成・編集ページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 * 
 * @param articleId - 編集モード時の記事ID
 * @param mode - 'create' または 'edit' (デフォルト: 'create')
 */
export function ArticleFormPage({ articleId, mode = 'create' }: ArticleFormPageProps = {}) {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [showPreview, setShowPreview] = useState(false);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);
  const [showPublishDialog, setShowPublishDialog] = useState(false);
  const [isPublishing, setIsPublishing] = useState(false); // 公開中かどうかを追跡
  
  const router = useRouter();
  const isEditMode = mode === 'edit' && articleId;
  
  // 編集モード時の記事データ取得
  const { data: article, isLoading: isLoadingArticle, error: articleError, refetch } = useGetArticle(
    { id: articleId || 0 },
    { enabled: !!isEditMode }
  );
  
  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      setHasUnsavedChanges(false);
      // 公開中フラグで判定
      if (isPublishing) {
        router.push(`/articles/${data.id}`);
      } else {
        router.push(`/articles/drafts`);
      }
      setIsPublishing(false);
    },
    onError: (error) => {
      console.error('記事の投稿に失敗しました:', error);
      alert('記事の投稿に失敗しました。もう一度お試しください。');
      setIsPublishing(false);
    },
  });

  const updateArticle = useUpdateArticle({
    onSuccess: () => {
      setHasUnsavedChanges(false);
      // 更新後は記事ページへ遷移
      router.push(`/articles/${articleId}`);
      setIsPublishing(false);
    },
    onError: (error) => {
      console.error('記事の更新に失敗しました:', error);
      alert('記事の更新に失敗しました。もう一度お試しください。');
      setIsPublishing(false);
    },
  });

  // 編集モード時のデータ初期化
  useEffect(() => {
    if (isEditMode && article && !isInitialized) {
      setTitle(article.title || '');
      setBody(article.body || '');
      setTags(article.tags || []);
      setIsInitialized(true);
    }
  }, [isEditMode, article, isInitialized]);

  const characterCount = body.length;
  const maxCharacters = 65535;
  const isOverLimit = characterCount > maxCharacters;

  // フォームの変更を追跡
  useEffect(() => {
    if (isInitialized && (title || body || tags.length > 0)) {
      setHasUnsavedChanges(true);
    }
  }, [title, body, tags, isInitialized]);

  // ページ離脱時の確認
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (hasUnsavedChanges && !createArticle.isSuccess && !updateArticle.isSuccess) {
        e.preventDefault();
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [hasUnsavedChanges, createArticle.isSuccess, updateArticle.isSuccess]);

  // ローディング状態
  if (isEditMode && isLoadingArticle) {
    return <Loading text="記事を読み込み中..." />;
  }

  // エラー状態
  if (isEditMode && articleError) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  // 記事が見つからない場合
  if (isEditMode && !article) {
    return <ErrorMessage message="記事が見つかりません" />;
  }

  const validateForm = (): boolean => {
    if (!title.trim()) {
      alert('タイトルを入力してください');
      return false;
    }

    if (tags.length === 0) {
      alert('少なくとも1つのタグを追加してください');
      return false;
    }

    if (body.length > maxCharacters) {
      alert(`文字数が上限を超えています。${maxCharacters.toLocaleString()}文字以内に収めてください。`);
      return false;
    }

    return true;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // フォーム送信は使用しない（ボタンで個別に処理）
  };

  const handlePublish = (publishedAt?: string) => {
    setIsPublishing(true); // 公開中フラグを設定
    if (isEditMode) {
      // 編集モード: 記事を更新
      updateArticle.mutate({
        id: articleId,
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: true,
        publishedAt: publishedAt,
      });
    } else {
      // 作成モード: 新規記事を作成
      createArticle.mutate({
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: true,
        publishedAt: publishedAt,
      });
    }
    
    // ダイアログを閉じる
    setShowPublishDialog(false);
  };

  const handleSave = () => {
    if (!validateForm()) {
      return;
    }

    if (isEditMode) {
      // 編集モード: 記事を更新（公開状態を保持）
      updateArticle.mutate({
        id: articleId,
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: article?.isPublished || false,
        publishedAt: article?.publishedAt, // 既存のpublishedAtを保持
      });
    } else {
      // 作成モード: 下書きとして保存（publishedAtなし）
      createArticle.mutate({
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: false,
        publishedAt: undefined,
      });
    }
  };

  const handleOpenPublishSettings = () => {
    if (!validateForm()) {
      return;
    }

    // バリデーションが通ったら、投稿設定ダイアログを開く
    setShowPublishDialog(true);
  };

  const togglePreview = () => {
    setShowPreview(!showPreview);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-2">
        <Label htmlFor="title">
          タイトル <span className="text-red-500">*</span>
        </Label>
        <Input
          id="title"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="記事のタイトルを入力してください"
          required
        />
      </div>

      <TagInput tags={tags} onTagsChange={setTags} />

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="body">
            本文 <span className="text-red-500">*</span>
          </Label>
          <div className="flex items-center gap-4">
            <CharacterCounter count={characterCount} max={maxCharacters} />
            <Button
              type="button"
              onClick={togglePreview}
              variant="secondary"
              size="sm"
            >
              {showPreview ? '編集モード' : 'プレビュー'}
            </Button>
          </div>
        </div>
        
        {isOverLimit && (
          <p className="text-sm text-red-500">
            文字数が上限を超えています。{maxCharacters.toLocaleString()}文字以内に収めてください。
          </p>
        )}
        
        {showPreview ? (
          <div className="min-h-[400px] px-4 py-3 border border-gray-300 rounded-lg bg-white markdown-preview">
            <ReactMarkdown>{body}</ReactMarkdown>
          </div>
        ) : (
          <div data-color-mode="light">
            <MDEditor
              value={body}
              onChange={(value) => setBody(value || '')}
              height={400}
              preview="edit"
              hideToolbar={false}
              visibleDragbar={false}
            />
          </div>
        )}
      </div>

      <div className="flex gap-4 pt-4">
        <Button
          type="button"
          onClick={handleSave}
          disabled={createArticle.isPending || updateArticle.isPending || !title || tags.length === 0 || isOverLimit}
          variant="outline"
          className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
        >
          {createArticle.isPending || updateArticle.isPending ? '保存中...' : '保存'}
        </Button>
        <Button
          type="button"
          onClick={handleOpenPublishSettings}
          disabled={createArticle.isPending || updateArticle.isPending || !title || tags.length === 0 || isOverLimit}
          className="flex-1 bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          投稿設定
        </Button>
      </div>

      <PublishSettingsDialog
        open={showPublishDialog}
        onOpenChange={setShowPublishDialog}
        onPublish={handlePublish}
        isLoading={createArticle.isPending || updateArticle.isPending}
      />
    </form>
  );
}
