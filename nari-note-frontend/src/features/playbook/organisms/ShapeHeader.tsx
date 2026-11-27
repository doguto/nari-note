'use client';

import type { ReactNode } from 'react';
import { CircleDot, MoreHorizontal, Sparkles, Split, Trash2, Unlink } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

interface ShapeHeaderProps {
  // 表示中の局面を見分けるキー。局面が変わったら入力欄を作り直す
  sfen: string;
  name: string;
  boardCount: number;
  nameSuggestions: string[];
  isNode: boolean;
  isRoot: boolean;
  isBusy: boolean;
  onRename: (name: string) => void;
  onMakeNode: () => void;
  onSplit: () => void;
  onUnlink: () => void;
  onDeleteNode: () => void;
  onDeleteTransition: () => void;
}

interface MenuItemProps {
  icon: ReactNode;
  title: string;
  description: string;
  disabled: boolean;
  danger?: boolean;
  onSelect: () => void;
}

function MenuItem({ icon, title, description, disabled, danger, onSelect }: MenuItemProps) {
  return (
    <DropdownMenuItem disabled={disabled} onSelect={onSelect} className="flex items-start gap-3 py-2">
      {icon}
      <span className="flex flex-col">
        <span className={danger ? 'font-bold text-red-700' : 'font-bold'}>{title}</span>
        <span className="text-xs text-brand-secondary-text">{description}</span>
      </span>
    </DropdownMenuItem>
  );
}

// 辺の途中の局面は、ノードにするか名前を付けるとグラフの点になる
export function ShapeHeader({
  sfen,
  name,
  boardCount,
  nameSuggestions,
  isNode,
  isRoot,
  isBusy,
  onRename,
  onMakeNode,
  onSplit,
  onUnlink,
  onDeleteNode,
  onDeleteTransition,
}: ShapeHeaderProps) {
  return (
    <section aria-label="この形" className="flex flex-col gap-2 rounded-xl border border-brand-border bg-white px-4 py-3 shadow-sm">
      <div className="flex items-center gap-2">
        <input
          key={`${sfen}-${name}`}
          type="text"
          defaultValue={name}
          placeholder={isNode ? 'この形に名前を付ける' : '名前を付けるとノードになります'}
          maxLength={100}
          aria-label="この形の名前"
          className="-ml-2 h-10 min-w-0 flex-1 rounded-md border border-transparent bg-transparent px-2 text-lg font-bold text-brand-text placeholder:font-normal placeholder:text-gray-400 hover:border-brand-border focus:border-brand-border focus:outline-none"
          onBlur={(e) => {
            if (e.target.value !== name) onRename(e.target.value);
          }}
        />
        {boardCount > 1 && (
          <span className="flex-none rounded-full bg-brand-primary-light px-2 py-0.5 text-xs font-bold text-brand-primary-dark">{boardCount}局面</span>
        )}
        {!isNode && (
          <Button variant="outline" size="sm" disabled={isBusy} className="flex-none" onClick={onMakeNode}>
            <CircleDot aria-hidden="true" />
            ノードにする
          </Button>
        )}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" size="icon" aria-label="形の操作" className="flex-none">
              <MoreHorizontal className="h-4 w-4" aria-hidden="true" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-64">
            {isNode ? (
              <>
                <MenuItem
                  icon={<Split className="mt-0.5 h-4 w-4 text-brand-primary-dark" aria-hidden="true" />}
                  title="この局面を形から分ける"
                  description="まとめた形から外して、別の形にする"
                  disabled={isBusy || boardCount <= 1}
                  onSelect={onSplit}
                />
                <MenuItem
                  icon={<Unlink className="mt-0.5 h-4 w-4 text-brand-primary-dark" aria-hidden="true" />}
                  title="ノードを解除する"
                  description="手順の途中に戻し、前後の辺を 1 本につなげる"
                  disabled={isBusy || isRoot}
                  onSelect={onUnlink}
                />
                <MenuItem
                  icon={<Trash2 className="mt-0.5 h-4 w-4 text-red-700" aria-hidden="true" />}
                  title="ノードを削除する"
                  description="このノードと、その先にしか無い手順を消す"
                  disabled={isBusy || isRoot}
                  danger
                  onSelect={onDeleteNode}
                />
              </>
            ) : (
              <MenuItem
                icon={<Trash2 className="mt-0.5 h-4 w-4 text-red-700" aria-hidden="true" />}
                title="この辺を削除する"
                description="この局面を通る手順と、その先にしか無い手順を消す"
                disabled={isBusy}
                danger
                onSelect={onDeleteTransition}
              />
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
      {nameSuggestions.length > 0 && (
        <div className="flex flex-wrap items-center gap-1.5">
          <span className="flex items-center gap-1 text-xs text-brand-secondary-text">
            <Sparkles className="h-3.5 w-3.5" aria-hidden="true" />
            候補
          </span>
          {nameSuggestions.map((s) => (
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
