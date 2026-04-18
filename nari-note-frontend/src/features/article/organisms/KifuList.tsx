import { Button } from '@/components/ui/button';
import { MoreHorizontal, NotebookIcon, Pencil, Trash2 } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { KifuItem } from '../types/kifu';

interface KifuListProps {
  kifuList: KifuItem[];
  onAdd: () => void;
  onEdit: (index: number) => void;
  onDelete: (index: number) => void;
}

const countMoves = (text: string) =>
  text.split('\n').filter((line) => /^\d+\s/.test(line.trim())).length;

export function KifuList({ kifuList, onAdd, onEdit, onDelete }: KifuListProps) {
  return (
    <div className="space-y-2">
      <Button type="button" variant="outline" size="sm" onClick={onAdd}>
        棋譜を追加
      </Button>

      {kifuList.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 mt-2">
          {kifuList.map((kifu, index) => {
            const moves = countMoves(kifu.text);
            return (
              <div
                key={index}
                className="flex items-center justify-between border rounded-lg px-3 py-2 bg-gray-50 hover:bg-gray-100 transition-colors"
              >
                <div className="flex items-center gap-2 min-w-0">
                  <NotebookIcon size={16}/>
                  <div className="min-w-0">
                    <p className="text-sm font-medium truncate">{kifu.name}</p>
                    {moves > 0 && (
                      <p className="text-xs text-muted-foreground">{moves}手</p>
                    )}
                  </div>
                </div>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="h-7 w-7 ml-2 shrink-0"
                      aria-label="メニューを開く"
                    >
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onSelect={() => onEdit(index)}>
                      <Pencil className="h-4 w-4" />
                      編集
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onSelect={() => onDelete(index)}
                      className="text-red-500 focus:text-red-600 focus:bg-red-50"
                    >
                      <Trash2 className="h-4 w-4" />
                      削除
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
