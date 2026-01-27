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

/**
 * TagInput - Molecule Component
 * 
 * タグ入力コンポーネント（入力フィールド + タグ一覧）
 */
export function TagInput({ tags, onTagsChange, required = true }: TagInputProps) {
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
          onChange={(e) => setTagInput(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="タグを入力してEnterで追加"
          className="flex-1"
        />
        <Button
          type="button"
          onClick={handleAddTag}
          className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
        >
          追加
        </Button>
      </div>
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
