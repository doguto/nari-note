'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { TagInput } from '@/components/common/molecules';
import { useCreateArticle, useUpdateArticle, useGetArticle } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/common/atoms';
import { PublishSettingsDialog } from './PublishSettingsDialog';
import {
  ArticleTitleInput,
  ArticleBodyEditor,
  ArticleFormActions,
} from '../molecules';

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
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);
  const [showPublishDialog, setShowPublishDialog] = useState(true);
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
    return <LoadingSpinner text="記事を読み込み中..." />;
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

  // 編集モード時は初期化完了まで待つ
  const isFormDisabled = (isEditMode && !isInitialized) || !title || tags.length === 0 || isOverLimit;
  const isLoading = createArticle.isPending || updateArticle.isPending;

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <ArticleTitleInput value={title} onChange={setTitle} />

      <TagInput tags={tags} onTagsChange={setTags} />

      <ArticleBodyEditor
        value={body}
        onChange={setBody}
        maxCharacters={maxCharacters}
      />

      <ArticleFormActions
        onSave={handleSave}
        onOpenPublishSettings={handleOpenPublishSettings}
        isLoading={isLoading}
        isDisabled={isFormDisabled}
      />

      <PublishSettingsDialog
        open={showPublishDialog}
        onOpenChange={setShowPublishDialog}
        onPublish={handlePublish}
        isLoading={createArticle.isPending || updateArticle.isPending}
      />
    </form>
  );
}
