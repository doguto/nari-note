'use client';

import { useState, useEffect } from 'react';
import { useCreateArticle } from '@/lib/api';
import { useRouter } from 'next/navigation';

/**
 * 記事作成フォームのカスタムフック
 * 
 * 記事作成フォームのロジックを管理します。
 */
export function useArticleForm() {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [showPreview, setShowPreview] = useState(false);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  
  const router = useRouter();
  
  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      setHasUnsavedChanges(false);
      router.push(`/articles/${data.id}`);
    },
    onError: (error) => {
      console.error('記事の投稿に失敗しました:', error);
      alert('記事の投稿に失敗しました。もう一度お試しください。');
    },
  });

  // フォームの変更を追跡
  useEffect(() => {
    if (title || body || tags.length > 0) {
      setHasUnsavedChanges(true);
    }
  }, [title, body, tags]);

  // ページ離脱時の確認
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (hasUnsavedChanges && !createArticle.isSuccess) {
        e.preventDefault();
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [hasUnsavedChanges, createArticle.isSuccess]);

  // バリデーションロジックを抽出
  const validateForm = (): boolean => {
    if (!title.trim()) {
      alert('タイトルを入力してください');
      return false;
    }

    if (tags.length === 0) {
      alert('少なくとも1つのタグを追加してください');
      return false;
    }

    if (body.length > 65535) {
      alert(`文字数が上限を超えています。65,535文字以内に収めてください。`);
      return false;
    }

    return true;
  };

  const handleSubmit = () => {
    if (!validateForm()) {
      return;
    }

    // 記事を投稿（公開状態）
    createArticle.mutate({
      title: title.trim(),
      body: body,
      tags: tags,
      isPublished: true,
    });
  };

  const handleSaveDraft = () => {
    if (!validateForm()) {
      return;
    }

    // 記事を下書きとして保存（非公開状態）
    createArticle.mutate({
      title: title.trim(),
      body: body,
      tags: tags,
      isPublished: false,
    });
  };

  const togglePreview = () => {
    setShowPreview(!showPreview);
  };

  return {
    title,
    setTitle,
    body,
    setBody,
    tags,
    setTags,
    showPreview,
    togglePreview,
    handleSubmit,
    handleSaveDraft,
    isSubmitting: createArticle.isPending,
  };
}
