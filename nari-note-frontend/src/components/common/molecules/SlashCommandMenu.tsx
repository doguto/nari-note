'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { CommandMenuItem } from '@/components/common/atoms';
import {
  Heading1,
  Heading2,
  Heading3,
  List,
  ListOrdered,
  Code,
  Image,
  Table,
  Quote,
  Minus,
  CheckSquare,
  Grid3x3,
  PlayCircle,
  LucideIcon,
} from 'lucide-react';

export interface CommandItem {
  id: string;
  icon: LucideIcon;
  label: string;
  description: string;
  insertText: string;
}

const COMMAND_ITEMS: CommandItem[] = [
  {
    id: 'h1',
    icon: Heading1,
    label: '見出し1',
    description: '大見出しを挿入',
    insertText: '# ',
  },
  {
    id: 'h2',
    icon: Heading2,
    label: '見出し2',
    description: '中見出しを挿入',
    insertText: '## ',
  },
  {
    id: 'h3',
    icon: Heading3,
    label: '見出し3',
    description: '小見出しを挿入',
    insertText: '### ',
  },
  {
    id: 'ul',
    icon: List,
    label: '箇条書きリスト',
    description: '箇条書きリストを挿入',
    insertText: '- ',
  },
  {
    id: 'ol',
    icon: ListOrdered,
    label: '番号付きリスト',
    description: '番号付きリストを挿入',
    insertText: '1. ',
  },
  {
    id: 'code',
    icon: Code,
    label: 'コードブロック',
    description: 'コードブロックを挿入',
    insertText: '```\n\n```',
  },
  {
    id: 'quote',
    icon: Quote,
    label: '引用',
    description: '引用ブロックを挿入',
    insertText: '> ',
  },
  {
    id: 'hr',
    icon: Minus,
    label: '水平線',
    description: '水平線を挿入',
    insertText: '\n---\n',
  },
  {
    id: 'checkbox',
    icon: CheckSquare,
    label: 'チェックボックス',
    description: 'チェックボックスリストを挿入',
    insertText: '- [ ] ',
  },
  {
    id: 'table',
    icon: Table,
    label: 'テーブル',
    description: 'テーブルを挿入',
    insertText: '| 列1 | 列2 | 列3 |\n| --- | --- | --- |\n| セル1 | セル2 | セル3 |',
  },
  {
    id: 'image',
    icon: Image,
    label: '画像',
    description: '画像を挿入',
    insertText: '![代替テキスト](画像URL)',
  },
  {
    id: 'board',
    icon: Grid3x3,
    label: '盤面',
    description: '将棋の盤面フォーマットを挿入',
    insertText: '```shogi-board\n// 盤面の設定をここに記述\n```',
  },
  {
    id: 'kifu',
    icon: PlayCircle,
    label: '棋譜',
    description: '将棋の棋譜フォーマットを挿入',
    insertText: '```shogi-kifu\n// 棋譜をここに記述\n```',
  },
];

interface SlashCommandMenuProps {
  open: boolean;
  onClose: () => void;
  onSelect: (insertText: string) => void;
}

/**
 * SlashCommandMenu - Molecule Component
 * 
 * スラッシュコマンドメニュー
 * マークダウン要素や将棋盤面/棋譜の挿入をサポート
 */
export function SlashCommandMenu({
  open,
  onClose,
  onSelect,
}: SlashCommandMenuProps) {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [searchQuery, setSearchQuery] = useState('');

  const filteredItems = COMMAND_ITEMS.filter((item) =>
    item.label.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  useEffect(() => {
    if (open) {
      setSelectedIndex(0);
      setSearchQuery('');
    }
  }, [open]);

  useEffect(() => {
    if (!open) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        setSelectedIndex((prev) => 
          prev < filteredItems.length - 1 ? prev + 1 : prev
        );
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        setSelectedIndex((prev) => (prev > 0 ? prev - 1 : prev));
      } else if (e.key === 'Enter') {
        e.preventDefault();
        if (filteredItems[selectedIndex]) {
          const item = filteredItems[selectedIndex];
          onSelect(item.insertText);
          onClose();
        }
      } else if (e.key === 'Escape') {
        e.preventDefault();
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [open, selectedIndex, filteredItems, onSelect, onClose]);

  const handleSelect = (insertText: string) => {
    onSelect(insertText);
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[600px] overflow-hidden p-0">
        <DialogHeader className="px-6 pt-6 pb-4">
          <DialogTitle>コマンドを選択</DialogTitle>
        </DialogHeader>
        
        <div className="px-6 pb-4">
          <input
            type="text"
            placeholder="検索..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            autoFocus
          />
        </div>

        <div className="overflow-y-auto max-h-[400px] border-t border-gray-200">
          {filteredItems.length === 0 ? (
            <div className="px-6 py-8 text-center text-gray-500">
              該当するコマンドが見つかりません
            </div>
          ) : (
            filteredItems.map((item, index) => (
              <CommandMenuItem
                key={item.id}
                icon={item.icon}
                label={item.label}
                description={item.description}
                onClick={() => handleSelect(item.insertText)}
                isSelected={index === selectedIndex}
              />
            ))
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
