'use client';

import { SearchInput } from '@/components/common/atoms';

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}

/**
 * SearchBar - Molecule Component
 * 
 * 検索バーコンポーネント
 * SearchInputを使った検索入力機能
 */
export function SearchBar({ 
  value, 
  onChange, 
  placeholder = 'タイトルや本文から記事を検索...' 
}: SearchBarProps) {
  return (
    <div className="w-full max-w-2xl mx-auto mb-8">
      <SearchInput
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        className="w-full"
      />
      {value && (
        <p className="mt-2 text-sm text-gray-600">
          「{value}」で検索中...
        </p>
      )}
    </div>
  );
}
