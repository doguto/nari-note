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
  const [searchQuery, setSearchQuery] = useState<string>('');
  const previousValueRef = useRef<string>('');

  // 値の変更を監視してスラッシュコマンドを検知
  useEffect(() => {
    const prevValue = previousValueRef.current;
    
    // 値が増えた場合（何か入力された）
    if (value.length >= prevValue.length) {
      // スラッシュコマンドメニューが開いている場合
      if (showSlashCommand) {
        // スラッシュ以降のテキストを取得
        const textAfterSlash = value.substring(slashPosition);
        const newlineIndex = textAfterSlash.indexOf('\n');
        const query = newlineIndex === -1 ? textAfterSlash : textAfterSlash.substring(0, newlineIndex);
        
        // スペースや改行が入力されたら閉じる
        if (query.includes(' ') || query.includes('\n')) {
          setShowSlashCommand(false);
          setSearchQuery('');
        } else {
          setSearchQuery(query);
        }
      } else {
        // 新しく「/」が入力されたかチェック
        const diff = value.length - prevValue.length;
        
        if (diff === 1) {
          const addedChar = value.charAt(value.length - 1);
          
          // 「/」が入力された場合
          if (addedChar === '/') {
            // 行の先頭かどうかをチェック
            const currentText = value.substring(0, value.length);
            const lastNewlineIndex = currentText.lastIndexOf('\n');
            const currentLine = currentText.substring(lastNewlineIndex + 1);
            
            // 行の先頭または空白のみの後に「/」が入力された場合
            if (currentLine === '/' || currentLine.match(/^\s+\/$/)) {
              setSlashPosition(value.length);
              setShowSlashCommand(true);
              setSearchQuery('');
            }
          }
        }
      }
    } else {
      // 値が減った場合（削除された）
      if (showSlashCommand && value.length < slashPosition) {
        // スラッシュ自体が削除された
        setShowSlashCommand(false);
        setSearchQuery('');
      }
    }
    
    previousValueRef.current = value;
  }, [value, showSlashCommand, slashPosition]);

  const handleCommandSelect = (insertText: string) => {
    // スラッシュとその後の検索クエリを削除して挿入テキストに置き換え
    const beforeSlash = value.substring(0, slashPosition - 1);
    const afterCommand = value.substring(slashPosition + searchQuery.length);
    
    const newValue = beforeSlash + insertText + afterCommand;
    onChange(newValue);
    setShowSlashCommand(false);
    setSearchQuery('');
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
        <div data-color-mode="light" className="relative">
          <MDEditor
            value={value}
            onChange={(val) => onChange(val || '')}
            height={400}
            preview="edit"
            hideToolbar={false}
            visibleDragbar={false}
          />
          {showSlashCommand && (
            <div className="absolute top-[60px] left-[20px]">
              <SlashCommandMenu
                open={showSlashCommand}
                onClose={() => {
                  setShowSlashCommand(false);
                  setSearchQuery('');
                }}
                onSelect={handleCommandSelect}
                searchQuery={searchQuery}
              />
            </div>
          )}
        </div>
      )}
    </div>
  );
}
