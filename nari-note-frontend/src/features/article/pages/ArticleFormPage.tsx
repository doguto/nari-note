'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useCreateArticle, useUpdateArticle, useGetArticle } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { ArticleFormTemplate } from '../templates/ArticleFormTemplate';

interface ArticleFormPageProps {
  articleId?: number;
  mode?: 'create' | 'edit';
}

/**
 * ArticleFormPage - Page Component
 * 
 * 記事作成・編集ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
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
  const [showPublishDialog, setShowPublishDialog] = useState(false);
  const [isPublishing, setIsPublishing] = useState(false);
  
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

  const maxCharacters = 65535;

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

  const handlePublish = (publishedAt?: string) => {
    setIsPublishing(true);
    if (isEditMode) {
      updateArticle.mutate({
        id: articleId,
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: true,
        publishedAt: publishedAt,
      });
    } else {
      createArticle.mutate({
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: true,
        publishedAt: publishedAt,
      });
    }
    
    setShowPublishDialog(false);
  };

  const handleSave = () => {
    if (!validateForm()) {
      return;
    }

    if (isEditMode) {
      updateArticle.mutate({
        id: articleId,
        title: title.trim(),
        body: body,
        tags: tags,
        isPublished: article?.isPublished || false,
        publishedAt: article?.publishedAt,
      });
    } else {
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

    setShowPublishDialog(true);
  };

  // フォームの有効性判定
  const hasValidTitle = !!title;
  const hasValidTags = tags.length > 0;
  const isWithinLimit = body.length <= maxCharacters;
  const isInitializationComplete = !isEditMode || isInitialized;
  
  const isFormDisabled = !isInitializationComplete || !hasValidTitle || !hasValidTags || !isWithinLimit;
  const isLoading = createArticle.isPending || updateArticle.isPending;

  return (
    <ArticleFormTemplate
      title={title}
      body={body}
      tags={tags}
      maxCharacters={maxCharacters}
      showPublishDialog={showPublishDialog}
      isLoading={isLoading}
      isFormDisabled={isFormDisabled}
      onTitleChange={setTitle}
      onBodyChange={setBody}
      onTagsChange={setTags}
      onSave={handleSave}
      onOpenPublishSettings={handleOpenPublishSettings}
      onPublish={handlePublish}
      onPublishDialogChange={setShowPublishDialog}
    />
  );
}
