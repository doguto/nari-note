'use client';

import { SearchInput } from '@/components/ui';
import { Button } from '@/components/ui/button';

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  onSearch: () => void;
  placeholder?: string;
}

/**
 * SearchBar - Molecule Component
 * 
 * 検索バーコンポーネント
 * SearchInputと検索ボタンを組み合わせた検索入力機能
 */
export function SearchBar({ 
  value, 
  onChange, 
  onSearch,
  placeholder = 'タイトルや本文から記事を検索...' 
}: SearchBarProps) {
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      onSearch();
    }
  };

  return (
    <div className="w-full w-3/4 mx-auto mb-8">
      <div className="flex gap-2">
        <div className="flex-1">
          <SearchInput
            value={value}
            onChange={onChange}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            className="w-full"
          />
        </div>
        <Button 
          onClick={onSearch}
          className="bg-brand-primary hover:bg-brand-primary-hover text-white px-6"
        >
          検索
        </Button>
      </div>
    </div>
  );
}
