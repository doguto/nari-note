import Link from 'next/link';
import { ChevronRight, LayoutGrid, Network, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ErrorAlert } from '@/components/ui';
import { ShogiBoard } from '@/lib/kif-player';
import type { PieceOwner } from '@/lib/kif-player';
import type { BookSummaryDto } from '@/lib/api/types';
import { cn } from '@/lib/utils';

interface MyBooksTemplateProps {
  books: BookSummaryDto[];
  name: string;
  rootBod: string | null;
  rootTurn: PieceOwner;
  isCreating: boolean;
  errorMessage?: string;
  onNameChange: (value: string) => void;
  onEditRoot: () => void;
  onResetRoot: () => void;
  onRootTurnChange: (turn: PieceOwner) => void;
  onCreate: () => void;
  onDelete: (book: BookSummaryDto) => void;
}

export function MyBooksTemplate({
  books,
  name,
  rootBod,
  rootTurn,
  isCreating,
  errorMessage,
  onNameChange,
  onEditRoot,
  onResetRoot,
  onRootTurnChange,
  onCreate,
  onDelete,
}: MyBooksTemplateProps) {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-bold text-gray-900 sm:text-3xl">マイ定跡</h1>
        <p className="mt-2 text-sm text-brand-secondary-text">研究盤で指した手がそのまま定跡になります。同じ形の局面は候補として提案されます。</p>
      </div>

      {errorMessage && <ErrorAlert message={errorMessage} />}

      <form
        className="rounded-lg bg-white p-4 shadow sm:p-6"
        onSubmit={(e) => {
          e.preventDefault();
          onCreate();
        }}
      >
        <h2 className="mb-4 text-lg font-bold text-brand-text">新しい定跡を作る</h2>
        <div className="flex flex-col gap-3 sm:flex-row">
          <label className="min-w-0 flex-1">
            <span className="sr-only">定跡の名前</span>
            <input
              type="text"
              required
              maxLength={100}
              value={name}
              placeholder="定跡の名前（例: 角換わり腰掛け銀）"
              className="h-10 w-full rounded-md border border-brand-border px-3 text-sm focus:outline-none focus:ring-2 focus:ring-brand-primary-light"
              onChange={(e) => onNameChange(e.target.value)}
            />
          </label>
          <Button
            type="submit"
            disabled={isCreating || name.trim().length === 0}
            className="h-10 bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
          >
            <Plus className="mr-1 h-4 w-4" />
            作成して研究盤を開く
          </Button>
        </div>

        <div className="mt-4 border-t border-gray-100 pt-4">
          {rootBod === null ? (
            <div className="flex flex-wrap items-center gap-3 text-sm">
              <span className="text-brand-secondary-text">開始局面: 平手の初期局面</span>
              <Button type="button" variant="outline" size="sm" onClick={onEditRoot}>
                <LayoutGrid className="mr-1 h-4 w-4" />
                途中の局面から始める
              </Button>
            </div>
          ) : (
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
              <ShogiBoard bodText={rootBod} size="sm" showPlayerNames={false} />
              <div className="flex flex-col gap-3 text-sm">
                <p className="font-bold text-brand-text">この局面から始めます</p>
                <fieldset className="flex flex-col gap-1">
                  <legend className="mb-1 text-xs text-brand-secondary-text">次に指すのは</legend>
                  <div className="inline-flex w-fit rounded-md border border-brand-border p-0.5">
                    {(['sente', 'gote'] as const).map((turn) => (
                      <button
                        key={turn}
                        type="button"
                        aria-pressed={rootTurn === turn}
                        className={cn(
                          'rounded px-4 py-1.5 text-sm transition-colors',
                          rootTurn === turn ? 'bg-brand-primary-dark text-white' : 'text-brand-text hover:bg-brand-primary-light',
                        )}
                        onClick={() => onRootTurnChange(turn)}
                      >
                        {turn === 'sente' ? '先手' : '後手'}
                      </button>
                    ))}
                  </div>
                </fieldset>
                <div className="flex gap-2">
                  <Button type="button" variant="outline" size="sm" onClick={onEditRoot}>
                    局面を編集
                  </Button>
                  <Button type="button" variant="ghost" size="sm" onClick={onResetRoot}>
                    平手に戻す
                  </Button>
                </div>
              </div>
            </div>
          )}
        </div>
      </form>

      <div className="rounded-lg bg-white shadow">
        <div className="border-b border-gray-200 px-4 py-4 sm:px-6">
          <h2 className="font-bold text-brand-text">定跡一覧（{books.length}）</h2>
        </div>
        {books.length === 0 ? (
          <div className="px-4 py-16 text-center">
            <Network className="mx-auto mb-4 h-16 w-16 text-gray-400" aria-hidden="true" />
            <p className="text-lg text-gray-500">まだ定跡がありません</p>
            <p className="mt-2 text-sm text-gray-500">上のフォームから定跡を作ると、研究盤が開きます。</p>
          </div>
        ) : (
          <ul className="divide-y divide-gray-200">
            {books.map((b) => (
              <li key={b.id} className="flex items-center gap-3 px-4 py-4 sm:px-6">
                <Link href={`/books/${b.id}`} className="group flex min-w-0 flex-1 items-center gap-3">
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-lg font-bold text-gray-900 transition-colors group-hover:text-[var(--brand-primary)]">
                      {b.name}
                    </p>
                    <p className="mt-1 text-sm text-gray-600">
                      {b.positionCount}局面 ・ 更新 {new Date(b.updatedAt).toLocaleDateString('ja-JP')}
                    </p>
                  </div>
                  <ChevronRight className="h-5 w-5 flex-none text-gray-400 group-hover:text-[var(--brand-primary)]" aria-hidden="true" />
                </Link>
                <Button
                  variant="outline"
                  size="sm"
                  aria-label={`${b.name}を削除`}
                  className="border-red-500 text-red-500 hover:bg-red-50"
                  onClick={() => onDelete(b)}
                >
                  <Trash2 className="h-4 w-4 sm:mr-1" />
                  <span className="hidden sm:inline">削除</span>
                </Button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
