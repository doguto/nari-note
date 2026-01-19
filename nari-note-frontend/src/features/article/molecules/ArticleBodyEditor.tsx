'use client';

import { useState, useRef, useEffect } from 'react';
import dynamic from 'next/dynamic';
import ReactMarkdown from 'react-markdown';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { CharacterCounter, SlashCommandMenu } from '@/components/common/molecules';

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
  const [showSlashCommand, setShowSlashCommand] = useState(false);
  const [slashPosition, setSlashPosition] = useState<number>(0);
  const editorRef = useRef<HTMLDivElement>(null);
  const previousValueRef = useRef<string>('');

  // 値の変更を監視してスラッシュコマンドを検知
  useEffect(() => {
    const prevValue = previousValueRef.current;
    
    // 値が増えた場合（何か入力された）
    if (value.length > prevValue.length) {
      const diff = value.length - prevValue.length;
      
      // 1文字だけ追加された場合
      if (diff === 1) {
        const addedChar = value.charAt(value.length - 1);
        
        // 「/」が入力された場合
        if (addedChar === '/') {
          // 行の先頭かどうかをチェック
          const textBeforeCursor = value.substring(0, value.length);
          const lastNewlineIndex = textBeforeCursor.lastIndexOf('\n');
          const currentLine = textBeforeCursor.substring(lastNewlineIndex + 1);
          
          // 行の先頭または空白のみの後に「/」が入力された場合
          if (currentLine === '/' || currentLine.match(/^\s+\/$/)) {
            setSlashPosition(value.length);
            setShowSlashCommand(true);
          }
        }
      }
    }
    
    previousValueRef.current = value;
  }, [value]);

  const handleCommandSelect = (insertText: string) => {
    // スラッシュを削除して挿入テキストに置き換え
    const beforeSlash = value.substring(0, slashPosition - 1);
    const afterSlash = value.substring(slashPosition);
    
    const newValue = beforeSlash + insertText + afterSlash;
    onChange(newValue);
    setShowSlashCommand(false);
  };

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
            ref={editorRef}
            value={value}
            onChange={(val) => onChange(val || '')}
            height={400}
            preview="edit"
            hideToolbar={false}
            visibleDragbar={false}
          />
        </div>
      )}

      <SlashCommandMenu
        open={showSlashCommand}
        onClose={() => setShowSlashCommand(false)}
        onSelect={handleCommandSelect}
      />
    </div>
  );
}
