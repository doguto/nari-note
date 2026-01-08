'use client';

import dynamic from 'next/dynamic';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import ReactMarkdown from 'react-markdown';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { TagInput, CharacterCounter } from '@/components/common/molecules';
import { useCreateArticle } from '@/lib/api';

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

/**
 * ArticleFormPage - Organism Component
 * 
 * 記事作成ページの完全な機能を持つコンポーネント
 * Atomic Designパターンにおける Organism として、
 * ビジネスロジックと UI を統合
 */
export function ArticleFormPage() {
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

  const characterCount = body.length;
  const maxCharacters = 65535;
  const isOverLimit = characterCount > maxCharacters;

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
    
    if (!validateForm()) {
      return;
    }

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
          onClick={handleSaveDraft}
          disabled={createArticle.isPending || !title || tags.length === 0 || isOverLimit}
          variant="outline"
          className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
        >
          下書き保存
        </Button>
        <Button
          type="submit"
          disabled={createArticle.isPending || !title || tags.length === 0 || isOverLimit}
          className="flex-1 bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          {createArticle.isPending ? '投稿中...' : '投稿する'}
        </Button>
      </div>
    </form>
  );
}
