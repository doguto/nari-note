'use client';

import { useState } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { TagChip } from '@/components/ui';

interface TagInputProps {
  tags: string[];
  onTagsChange: (tags: string[]) => void;
  required?: boolean;
}

const VALID_TAG_PATTERN = /^[a-zA-Z0-9\u3040-\u30FF\u4E00-\u9FFF_.\-]+$/;

/**
 * TagInput - Molecule Component
 * 
 * タグ入力コンポーネント（入力フィールド + タグ一覧）
 */
export function TagInput({ tags, onTagsChange, required = true }: TagInputProps) {
  const [tagInput, setTagInput] = useState('');
  const [tagError, setTagError] = useState('');

  const handleAddTag = () => {
    const trimmed = tagInput.trim();
    if (!trimmed) return;

    if (!VALID_TAG_PATTERN.test(trimmed)) {
      setTagError('タグ名に使用できない文字が含まれています。英数字・日本語・ハイフン(-)・アンダースコア(_)・ピリオド(.)のみ使用できます。');
      return;
    }

    if (tags.includes(trimmed)) {
      setTagError('同じタグがすでに追加されています。');
      return;
    }

    onTagsChange([...tags, trimmed]);
    setTagInput('');
    setTagError('');
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

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setTagInput(e.target.value);
    if (tagError) setTagError('');
  };

  return (
    <div className="space-y-2">
      <Label htmlFor="tags">
        タグ {required && <span className="text-red-500">*</span>}
      </Label>
      <div className="flex gap-2 mb-2">
        <Input
          id="tags"
          type="text"
          value={tagInput}
          onChange={handleInputChange}
          onKeyPress={handleKeyPress}
          placeholder="タグを入力してEnterで追加"
          className="flex-1"
          aria-describedby={tagError ? 'tag-error' : undefined}
        />
        <Button
          type="button"
          onClick={handleAddTag}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          追加
        </Button>
      </div>
      {tagError && (
        <p id="tag-error" className="text-sm text-red-500" role="alert">{tagError}</p>
      )}
      {tags.length > 0 && (
        <div className="flex gap-2 flex-wrap">
          {tags.map((tag) => (
            <TagChip key={tag} tag={tag} onRemove={handleRemoveTag} />
          ))}
        </div>
      )}
      {tags.length === 0 && (
        <p className="text-sm text-gray-500">少なくとも1つのタグを追加してください</p>
      )}
    </div>
  );
}
