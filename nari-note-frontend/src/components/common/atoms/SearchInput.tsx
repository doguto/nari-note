import { Search } from 'lucide-react';
import { Input } from '@/components/ui';

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  onKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  placeholder?: string;
  className?: string;
}

/**
 * SearchInput - Atom Component
 * 
 * 検索入力フィールドの最小単位コンポーネント
 * 検索アイコン付きのインプットフィールド
 */
export function SearchInput({ 
  value, 
  onChange, 
  onKeyDown,
  placeholder = '検索...',
  className = ''
}: SearchInputProps) {
  return (
    <div className={`relative ${className}`}>
      <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
      <Input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={onKeyDown}
        placeholder={placeholder}
        className="pl-10 bg-white text-gray-900"
      />
    </div>
  );
}
