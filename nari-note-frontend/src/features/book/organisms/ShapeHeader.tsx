'use client';

import { MoreHorizontal, Sparkles, Split } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { ShapeGroupDto } from '@/lib/api/types';

interface ShapeHeaderProps {
  group: ShapeGroupDto;
  isBusy: boolean;
  onRename: (name: string) => void;
  onSplit: () => void;
}

export function ShapeHeader({ group, isBusy, onRename, onSplit }: ShapeHeaderProps) {
  const positionCount = group.members.length;

  return (
    <section aria-label="この形" className="flex flex-col gap-2 rounded-xl border border-brand-border bg-white px-4 py-3 shadow-sm">
      <div className="flex items-center gap-2">
        <input
          key={`${group.id}-${group.name}`}
          type="text"
          defaultValue={group.name}
          placeholder="この形に名前を付ける"
          maxLength={100}
          aria-label="この形の名前"
          className="-ml-2 h-10 min-w-0 flex-1 rounded-md border border-transparent bg-transparent px-2 text-lg font-bold text-brand-text placeholder:font-normal placeholder:text-gray-400 hover:border-brand-border focus:border-brand-border focus:outline-none"
          onBlur={(e) => {
            if (e.target.value !== group.name) onRename(e.target.value);
          }}
        />
        {positionCount > 1 && (
          <span className="flex-none rounded-full bg-brand-primary-light px-2 py-0.5 text-xs font-bold text-brand-primary-dark">
            {positionCount}局面
          </span>
        )}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" size="icon" aria-label="形の操作" className="flex-none">
              <MoreHorizontal className="h-4 w-4" aria-hidden="true" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-64">
            <DropdownMenuItem disabled={isBusy || positionCount <= 1} onSelect={onSplit} className="flex items-start gap-3 py-2">
              <Split className="mt-0.5 h-4 w-4 text-brand-primary-dark" aria-hidden="true" />
              <span className="flex flex-col">
                <span className="font-bold">この局面を形から分ける</span>
                <span className="text-xs text-brand-secondary-text">まとめた形から外して、別の形にする</span>
              </span>
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
      {group.nameSuggestions.length > 0 && (
        <div className="flex flex-wrap items-center gap-1.5">
          <span className="flex items-center gap-1 text-xs text-brand-secondary-text">
            <Sparkles className="h-3.5 w-3.5" aria-hidden="true" />
            候補
          </span>
          {group.nameSuggestions.map((s) => (
            <button
              key={s}
              type="button"
              disabled={isBusy}
              className="rounded-full border border-dashed border-brand-border px-2.5 py-0.5 text-xs text-brand-text hover:border-brand-primary-dark hover:bg-brand-primary-light disabled:opacity-50"
              onClick={() => onRename(s)}
            >
              {s}
            </button>
          ))}
        </div>
      )}
    </section>
  );
}
