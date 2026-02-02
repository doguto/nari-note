'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useCreateCourse, useUpdateCourse, useGetCourseContent } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { FormPageLayout } from '@/components/molecules';
import { CourseFormTemplate } from '../templates/CourseFormTemplate';

interface CourseFormPageProps {
  courseId?: number;
  mode?: 'create' | 'edit';
}

/**
 * CourseFormPage - Page Component
 * 
 * 講座作成・編集ページのロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 * 
 * @param courseId - 編集モード時の講座ID
 * @param mode - 'create' または 'edit' (デフォルト: 'create')
 */
export function CourseFormPage({ courseId, mode = 'create' }: CourseFormPageProps = {}) {
  const [name, setName] = useState('');
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);
  const [showPublishDialog, setShowPublishDialog] = useState(false);
  const [isPublishing, setIsPublishing] = useState(false);
  
  const router = useRouter();
  const isEditMode = mode === 'edit' && courseId;
  
  // 編集モード時の講座データ取得
  const { data: course, isLoading: isLoadingCourse, error: courseError, refetch } = useGetCourseContent(
    { id: courseId || 0 },
    { enabled: !!isEditMode }
  );
  
  const createCourse = useCreateCourse({
    onSuccess: (data) => {
      setHasUnsavedChanges(false);
      if (isPublishing && data.course?.id) {
        router.push(`/courses/${data.course.id}`);
      } else {
        router.push('/courses');
      }
      setIsPublishing(false);
    },
    onError: (error) => {
      console.error('講座の作成に失敗しました:', error);
      alert('講座の作成に失敗しました。もう一度お試しください。');
      setIsPublishing(false);
    },
  });

  const updateCourse = useUpdateCourse({
    onSuccess: () => {
      setHasUnsavedChanges(false);
      router.push(`/courses/${courseId}`);
      setIsPublishing(false);
    },
    onError: (error) => {
      console.error('講座の更新に失敗しました:', error);
      alert('講座の更新に失敗しました。もう一度お試しください。');
      setIsPublishing(false);
    },
  });

  // 編集モード時のデータ初期化
  useEffect(() => {
    if (isEditMode && course && !isInitialized) {
      setName(course.name || '');
      setIsInitialized(true);
    }
  }, [isEditMode, course, isInitialized]);

  // フォームの変更を追跡
  useEffect(() => {
    if (isInitialized && name) {
      setHasUnsavedChanges(true);
    }
  }, [name, isInitialized]);

  // ページ離脱時の確認
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (hasUnsavedChanges && !createCourse.isSuccess && !updateCourse.isSuccess) {
        e.preventDefault();
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [hasUnsavedChanges, createCourse.isSuccess, updateCourse.isSuccess]);

  // ローディング状態
  if (isEditMode && isLoadingCourse) {
    return <LoadingSpinner text="講座を読み込み中..." />;
  }

  // エラー状態
  if (isEditMode && courseError) {
    return (
      <ErrorMessage 
        message="講座の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  // 講座が見つからない場合
  if (isEditMode && !course) {
    return <ErrorMessage message="講座が見つかりません" />;
  }

  const validateForm = (): boolean => {
    if (!name.trim()) {
      alert('講座名を入力してください');
      return false;
    }

    if (name.length > 100) {
      alert('講座名は100文字以内で入力してください');
      return false;
    }

    return true;
  };

  const handlePublish = (publishedAt?: string) => {
    setIsPublishing(true);
    if (isEditMode) {
      updateCourse.mutate({
        id: courseId,
        name: name.trim(),
        isPublished: true,
        publishedAt: publishedAt,
      });
    } else {
      createCourse.mutate({
        name: name.trim(),
      });
      // Note: 新規作成時はまだ公開設定をサポートしていないため、
      // 作成後に編集画面から公開設定を行う必要があります
    }
    
    setShowPublishDialog(false);
  };

  const handleSave = () => {
    if (!validateForm()) {
      return;
    }

    if (isEditMode) {
      updateCourse.mutate({
        id: courseId,
        name: name.trim(),
        isPublished: course?.isPublished || false,
        publishedAt: course?.publishedAt,
      });
    } else {
      createCourse.mutate({
        name: name.trim(),
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
  const hasValidName = !!name && name.length <= 100;
  const isInitializationComplete = !isEditMode || isInitialized;
  
  const isFormDisabled = !isInitializationComplete || !hasValidName;
  const isLoading = createCourse.isPending || updateCourse.isPending;

  const pageTitle = isEditMode ? '講座を編集' : '新規講座作成';
  const pageDescription = '講座を作成して、関連する記事をシリーズとしてまとめることができます。';

  return (
    <FormPageLayout title={pageTitle} description={pageDescription}>
      <CourseFormTemplate
        name={name}
        showPublishDialog={showPublishDialog}
        isLoading={isLoading}
        isFormDisabled={isFormDisabled}
        isEditMode={!!isEditMode}
        onNameChange={setName}
        onSave={handleSave}
        onOpenPublishSettings={handleOpenPublishSettings}
        onPublish={handlePublish}
        onPublishDialogChange={setShowPublishDialog}
      />
    </FormPageLayout>
  );
}
