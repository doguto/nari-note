'use client';

import dynamic from 'next/dynamic';
import ReactMarkdown from 'react-markdown';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { TagInput, CharacterCounter } from '@/components/common/molecules';

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

interface ArticleFormProps {
  title: string;
  body: string;
  tags: string[];
  onTitleChange: (value: string) => void;
  onBodyChange: (value: string) => void;
  onTagsChange: (tags: string[]) => void;
  onSubmit: () => void;
  onSaveDraft: () => void;
  isSubmitting: boolean;
  showPreview: boolean;
  onTogglePreview: () => void;
}

/**
 * ArticleForm - Organism Component
 * 
 * 記事作成・編集フォームを表示します。
 * Atomic Designパターンに基づいて、Atoms/Moleculesを組み合わせて構築
 */
export function ArticleForm({
  title,
  body,
  tags,
  onTitleChange,
  onBodyChange,
  onTagsChange,
  onSubmit,
  onSaveDraft,
  isSubmitting,
  showPreview,
  onTogglePreview,
}: ArticleFormProps) {
  const characterCount = body.length;
  const maxCharacters = 65535;
  const isOverLimit = characterCount > maxCharacters;

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSubmit(); }} className="space-y-6">
      <div className="space-y-2">
        <Label htmlFor="title">
          タイトル <span className="text-red-500">*</span>
        </Label>
        <Input
          id="title"
          type="text"
          value={title}
          onChange={(e) => onTitleChange(e.target.value)}
          placeholder="記事のタイトルを入力してください"
          required
        />
      </div>

      <TagInput tags={tags} onTagsChange={onTagsChange} />

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="body">
            本文 <span className="text-red-500">*</span>
          </Label>
          <div className="flex items-center gap-4">
            <CharacterCounter count={characterCount} max={maxCharacters} />
            <Button
              type="button"
              onClick={onTogglePreview}
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
              onChange={(value) => onBodyChange(value || '')}
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
          onClick={onSaveDraft}
          disabled={isSubmitting || !title || tags.length === 0 || isOverLimit}
          variant="outline"
          className="border-[var(--brand-primary)] text-[var(--brand-primary)] hover:bg-[var(--brand-bg-light)]"
        >
          下書き保存
        </Button>
        <Button
          type="submit"
          disabled={isSubmitting || !title || tags.length === 0 || isOverLimit}
          className="flex-1 bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          {isSubmitting ? '投稿中...' : '投稿する'}
        </Button>
      </div>
    </form>
  );
}
