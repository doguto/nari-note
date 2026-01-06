'use client';

import dynamic from 'next/dynamic';
import { useState } from 'react';
import ReactMarkdown from 'react-markdown';

// Dynamically import MDEditor to avoid SSR issues
const MDEditor = dynamic(
  () => import('@uiw/react-md-editor').then((mod) => mod.default),
  { ssr: false }
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
 * 記事作成フォーム - Presentational Component
 * 
 * 記事の作成・編集に使用するフォームコンポーネント。
 * マークダウンエディターとプレビュー機能を提供します。
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
  const [tagInput, setTagInput] = useState('');

  const handleAddTag = () => {
    if (tagInput.trim() && !tags.includes(tagInput.trim())) {
      onTagsChange([...tags, tagInput.trim()]);
      setTagInput('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    onTagsChange(tags.filter(tag => tag !== tagToRemove));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddTag();
    }
  };

  const characterCount = body.length;
  const maxCharacters = 65535;
  const isOverLimit = characterCount > maxCharacters;

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSubmit(); }} className="space-y-6">
      {/* タイトル入力 */}
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-[#2d3e1f] mb-2">
          タイトル <span className="text-red-500">*</span>
        </label>
        <input
          id="title"
          type="text"
          value={title}
          onChange={(e) => onTitleChange(e.target.value)}
          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#88b04b] focus:border-transparent"
          placeholder="記事のタイトルを入力してください"
          required
        />
      </div>

      {/* タグ入力 */}
      <div>
        <label htmlFor="tags" className="block text-sm font-medium text-[#2d3e1f] mb-2">
          タグ <span className="text-red-500">*</span>
        </label>
        <div className="flex gap-2 mb-2">
          <input
            id="tags"
            type="text"
            value={tagInput}
            onChange={(e) => setTagInput(e.target.value)}
            onKeyPress={handleKeyPress}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#88b04b] focus:border-transparent"
            placeholder="タグを入力してEnterで追加"
          />
          <button
            type="button"
            onClick={handleAddTag}
            className="px-4 py-2 bg-[#88b04b] text-white rounded-lg hover:bg-[#769939] transition-colors"
          >
            追加
          </button>
        </div>
        {tags.length > 0 && (
          <div className="flex gap-2 flex-wrap">
            {tags.map((tag) => (
              <span
                key={tag}
                className="px-3 py-1 bg-[#f5f3e8] text-[#2d3e1f] rounded-full text-sm flex items-center gap-2"
              >
                #{tag}
                <button
                  type="button"
                  onClick={() => handleRemoveTag(tag)}
                  className="text-red-500 hover:text-red-700"
                  aria-label={`${tag}を削除`}
                >
                  ×
                </button>
              </span>
            ))}
          </div>
        )}
        {tags.length === 0 && (
          <p className="text-sm text-gray-500">少なくとも1つのタグを追加してください</p>
        )}
      </div>

      {/* 本文入力 */}
      <div>
        <div className="flex items-center justify-between mb-2">
          <label htmlFor="body" className="block text-sm font-medium text-[#2d3e1f]">
            本文 <span className="text-red-500">*</span>
          </label>
          <div className="flex items-center gap-4">
            <span className={`text-sm ${isOverLimit ? 'text-red-500 font-bold' : 'text-gray-600'}`}>
              {characterCount.toLocaleString()} / {maxCharacters.toLocaleString()} 文字
            </span>
            <button
              type="button"
              onClick={onTogglePreview}
              className="px-3 py-1 bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition-colors text-sm"
            >
              {showPreview ? '編集モード' : 'プレビュー'}
            </button>
          </div>
        </div>
        
        {showPreview ? (
          <div 
            className="min-h-[400px] px-4 py-3 border border-gray-300 rounded-lg bg-white prose max-w-none"
          >
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
        
        {isOverLimit && (
          <p className="mt-2 text-sm text-red-500">
            文字数が上限を超えています。{maxCharacters.toLocaleString()}文字以内に収めてください。
          </p>
        )}
      </div>

      {/* 送信ボタン */}
      <div className="flex gap-4 pt-4">
        <button
          type="button"
          onClick={onSaveDraft}
          disabled={isSubmitting || !title || tags.length === 0 || isOverLimit}
          className="px-6 py-3 border border-[#88b04b] text-[#88b04b] rounded-lg hover:bg-[#f5f3e8] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          下書き保存
        </button>
        <button
          type="submit"
          disabled={isSubmitting || !title || tags.length === 0 || isOverLimit}
          className="flex-1 px-6 py-3 bg-[#88b04b] text-white rounded-lg hover:bg-[#769939] transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-medium"
        >
          {isSubmitting ? '投稿中...' : '投稿する'}
        </button>
      </div>
    </form>
  );
}
