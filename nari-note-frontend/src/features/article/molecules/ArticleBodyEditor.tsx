'use client';

import dynamic from 'next/dynamic';
import ReactMarkdown from 'react-markdown';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { CharacterCounter } from '@/components/common/molecules';

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

interface ArticleBodyEditorProps {
  value: string;
  onChange: (value: string) => void;
  showPreview: boolean;
  onTogglePreview: () => void;
  maxCharacters?: number;
}

/**
 * ArticleBodyEditor - Molecule Component
 * 
 * 記事本文エディター（Markdown編集とプレビュー機能付き）
 */
export function ArticleBodyEditor({
  value,
  onChange,
  showPreview,
  onTogglePreview,
  maxCharacters = 65535,
}: ArticleBodyEditorProps) {
  const characterCount = value.length;
  const isOverLimit = characterCount > maxCharacters;

  return (
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
          <ReactMarkdown>{value}</ReactMarkdown>
        </div>
      ) : (
        <div data-color-mode="light">
          <MDEditor
            value={value}
            onChange={(val) => onChange(val || '')}
            height={400}
            preview="edit"
            hideToolbar={false}
            visibleDragbar={false}
          />
        </div>
      )}
    </div>
  );
}
