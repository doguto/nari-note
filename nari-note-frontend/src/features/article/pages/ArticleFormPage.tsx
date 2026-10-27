'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useCreateArticle, useUpdateArticle, useGetArticleContent } from '@/lib/api';
import { PageWithoutSidebar } from '@/features/global/organisms';
import { ArticleFormTemplate } from '../templates/ArticleFormTemplate';
import { useAuth } from '@/lib/providers/AuthProvider';
import type { KifuDto } from '@/lib/api/types';
import type { KifuItem } from '../types/kifu';

const toKifuDtos = (items: KifuItem[]): KifuDto[] =>
  items.map((item, i) => ({ name: item.name, kifuText: item.text, sortOrder: i }));

// 予約投稿ではなく、保存と同時に公開されるか
const isPublishedImmediately = (request: { isPublished?: boolean; publishedAt?: string }) =>
  !!request.isPublished && (!request.publishedAt || new Date(request.publishedAt) <= new Date());

const fromKifuDtos = (dtos: KifuDto[]): KifuItem[] =>
  [...dtos].sort((a, b) => a.sortOrder - b.sortOrder).map((dto) => ({ name: dto.name, text: dto.kifuText }));

interface ArticleFormPageProps {
  articleId?: string;
  mode?: 'create' | 'edit';
  courseId?: string;
}


export function ArticleFormPage({ articleId, mode = 'create', courseId }: ArticleFormPageProps = {}) {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [kifuList, setKifuList] = useState<KifuItem[]>([]);
  const [editingKifuIndex, setEditingKifuIndex] = useState<number | null>(null);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);
  const [showPublishDialog, setShowPublishDialog] = useState(false);
  const [showKifuDialog, setShowKifuDialog] = useState(false);
  const [validationError, setValidationError] = useState<string>('');
  
  const router = useRouter();
  const { userId } = useAuth();
  const isEditMode = mode === 'edit' && articleId;

  // 編集モード時の記事データ取得
  const { data: article, isLoading: isLoadingArticle, error: articleError, refetch } = useGetArticleContent(
    { id: articleId || '' },
    { enabled: !!isEditMode }
  );
  
  const createArticle = useCreateArticle({
    onSuccess: (data, variables) => {
      setHasUnsavedChanges(false);
      router.push(`/articles/${data.id}${isPublishedImmediately(variables) ? '?published=1' : ''}`);
    },
    onError: (error) => {
      console.error('記事の保存に失敗しました:', error);
      setValidationError('記事の保存に失敗しました。もう一度お試しください。');
    },
  });

  const updateArticle = useUpdateArticle({
    onSuccess: (_, variables) => {
      setHasUnsavedChanges(false);
      // 下書きを公開したときだけ案内を出す（公開済み記事の更新では出さない）
      const isNewlyPublished = !article?.article?.isPublished && isPublishedImmediately(variables);
      router.push(`/articles/${articleId}${isNewlyPublished ? '?published=1' : ''}`);
    },
    onError: (error) => {
      console.error('記事の保存に失敗しました:', error);
      setValidationError('記事の保存に失敗しました。もう一度お試しください。');
    },
  });

  // 編集モード時のデータ初期化
  if (isEditMode && article && !isInitialized) {
    setTitle(article.article.title || '');
    setBody(article.article.body || '');
    setTags(article.article.tags || []);
    setKifuList(fromKifuDtos(article.article.kifus || []));
    setIsInitialized(true);
  }

  const maxCharacters = 65535;

  // フォームの変更を追跡
  const handleTitleChange = (value: string) => {
    setTitle(value);
    setHasUnsavedChanges(true);
  };

  const handleBodyChange = (value: string) => {
    setBody(value);
    setHasUnsavedChanges(true);
  };

  const handleTagsChange = (value: string[]) => {
    setTags(value);
    setHasUnsavedChanges(true);
  };

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

  const validateForm = (): boolean => {
    setValidationError('');

    if (!title.trim()) {
      setValidationError('タイトルを入力してください');
      return false;
    }

    if (tags.length === 0) {
      setValidationError('少なくとも1つのタグを追加してください');
      return false;
    }

    if (body.length > maxCharacters) {
      setValidationError(`文字数が上限を超えています。${maxCharacters.toLocaleString()}文字以内に収めてください。`);
      return false;
    }

    return true;
  };

  const handlePublish = (publishedAt?: string) => {
    setValidationError('');
    if (isEditMode) {
      updateArticle.mutate({
        id: articleId,
        title: title.trim(),
        body: body,
        tags: tags,
        kifus: toKifuDtos(kifuList),
        isPublished: true,
        publishedAt: publishedAt,
      });
    } else {
      createArticle.mutate({
        title: title.trim(),
        body: body,
        tags: tags,
        kifus: toKifuDtos(kifuList),
        isPublished: true,
        publishedAt: publishedAt,
        authorId: userId!,
        courseId: courseId,
      });
    }

    setShowPublishDialog(false);
  };

  const handleSave = () => {
    if (!validateForm()) {
      return;
    }

    setValidationError('');

    if (isEditMode) {
      updateArticle.mutate({
        id: articleId,
        title: title.trim(),
        body: body,
        tags: tags,
        kifus: toKifuDtos(kifuList),
        isPublished: article?.article?.isPublished || false,
        publishedAt: article?.article?.publishedAt,
      });
    } else {
      createArticle.mutate({
        title: title.trim(),
        body: body,
        tags: tags,
        kifus: toKifuDtos(kifuList),
        isPublished: false,
        publishedAt: undefined,
        authorId: userId!,
        courseId: courseId,
      });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
  };

  const handleKifuConfirm = (kifu: KifuItem) => {
    if (editingKifuIndex !== null) {
      setKifuList((prev) => prev.map((item, i) => (i === editingKifuIndex ? kifu : item)));
    } else {
      setKifuList((prev) => [...prev, kifu]);
    }
    setEditingKifuIndex(null);
  };

  const handleKifuDelete = (index: number) => {
    setKifuList((prev) => prev.filter((_, i) => i !== index));
    if (editingKifuIndex === index) setEditingKifuIndex(null);
  };

  const handleKifuEdit = (index: number) => {
    setEditingKifuIndex(index);
    setShowKifuDialog(true);
  };

  const handleKifuDialogChange = (open: boolean) => {
    setShowKifuDialog(open);
    if (!open) setEditingKifuIndex(null);
  };

  const handleOpenPublishSettings = () => {
    if (!validateForm()) {
      return;
    }

    setValidationError('');
    setShowPublishDialog(true);
  };

  // フォームの有効性判定
  const hasValidTitle = !!title;
  const hasValidTags = tags.length > 0;
  const isWithinLimit = body.length <= maxCharacters;
  const isInitializationComplete = !isEditMode || isInitialized;
  
  const isFormDisabled = !isInitializationComplete || !hasValidTitle || !hasValidTags || !isWithinLimit || !userId;
  const isLoading = createArticle.isPending || updateArticle.isPending;

  const pageTitle = isEditMode ? '記事を編集' : '新規記事作成';

  // コンテンツ読み込み状態とエラー状態を判定
  const isLoadingContent = Boolean(isEditMode && isLoadingArticle);
  
  const getContentError = (): string | undefined => {
    if (!isEditMode) return undefined;
    if (articleError) return '記事の取得に失敗しました';
    if (!article && !isLoadingArticle) return '記事が見つかりません';
    return undefined;
  };
  
  const contentError = getContentError();

  return (
    <PageWithoutSidebar title={pageTitle}>
      <ArticleFormTemplate
        title={title}
        body={body}
        tags={tags}
        kifuList={kifuList}
        editingKifuIndex={editingKifuIndex}
        maxCharacters={maxCharacters}
        showPublishDialog={showPublishDialog}
        showKifuDialog={showKifuDialog}
        isLoading={isLoading}
        isFormDisabled={isFormDisabled}
        isLoadingContent={isLoadingContent}
        contentError={contentError}
        onRetry={refetch}
        validationError={validationError}
        onTitleChange={handleTitleChange}
        onBodyChange={handleBodyChange}
        onTagsChange={handleTagsChange}
        onKifuConfirm={handleKifuConfirm}
        onKifuAdd={handleKifuConfirm}
        onKifuEdit={handleKifuEdit}
        onKifuDelete={handleKifuDelete}
        onSave={handleSave}
        onSubmit={handleSubmit}
        onOpenPublishSettings={handleOpenPublishSettings}
        onPublish={handlePublish}
        onPublishDialogChange={setShowPublishDialog}
        onKifuDialogChange={handleKifuDialogChange}
      />
    </PageWithoutSidebar>
  );
}
