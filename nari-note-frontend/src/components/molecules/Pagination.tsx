import Link from 'next/link';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationBaseProps {
  currentPage: number;
  basePath: string;
  /** page 以外に URL へ残すクエリ（タブの種類・検索キーワードなど） */
  query?: Record<string, string>;
}

/**
 * 総件数が分かる場合はページ番号を並べ、分からない場合は hasNext だけを渡して「前へ / 次へ」で送る
 */
type PaginationProps = PaginationBaseProps &
  (
    | { totalCount: number; pageSize: number; hasNext?: never }
    | { hasNext: boolean; totalCount?: never; pageSize?: never }
  );

type PageItem = { kind: 'page'; page: number } | { kind: 'ellipsis'; key: string };

/**
 * 先頭・末尾・現在ページの前後 siblings ページを並べ、間が 2 ページ以上空く箇所を省略記号にする
 */
function buildPageItems(currentPage: number, totalPages: number, siblings: number): PageItem[] {
  const center = Math.min(Math.max(currentPage, 1), totalPages);
  const pages = new Set([1, totalPages]);
  for (let page = center - siblings; page <= center + siblings; page++) {
    if (page >= 1 && page <= totalPages) pages.add(page);
  }

  const items: PageItem[] = [];
  let prev = 0;
  [...pages].sort((a, b) => a - b).forEach((page) => {
    if (page - prev === 2) {
      items.push({ kind: 'page', page: page - 1 });
    } else if (page - prev > 2) {
      items.push({ kind: 'ellipsis', key: `ellipsis-${page}` });
    }
    items.push({ kind: 'page', page });
    prev = page;
  });
  return items;
}

const baseClass = 'h-11 flex items-center justify-center rounded-lg border text-sm transition-colors';
const linkClass = `${baseClass} border-brand-border bg-white text-brand-text hover:border-brand-primary hover:bg-brand-bg-light`;
const currentClass = `${baseClass} border-brand-text bg-brand-text text-white font-bold`;
const disabledClass = `${baseClass} border-brand-border/60 bg-white/60 text-gray-400`;
const stepClass = 'w-11 sm:w-auto sm:px-3.5 gap-1 font-medium';

/**
 * Pagination - Molecule Component
 *
 * `?page=N` のクエリでページを切り替えるページ送り
 */
export function Pagination({ currentPage, basePath, query, totalCount, pageSize, hasNext: hasNextPage }: PaginationProps) {
  const totalPages = totalCount !== undefined && pageSize !== undefined ? Math.ceil(totalCount / pageSize) : undefined;
  const hasPrev = currentPage > 1;
  const hasNext = totalPages !== undefined ? currentPage < totalPages : !!hasNextPage;
  if (!hasPrev && !hasNext) return null;

  const hrefOf = (page: number) => {
    const params = new URLSearchParams(query);
    if (page !== 1) params.set('page', String(page));
    const search = params.toString();
    return search ? `${basePath}?${search}` : basePath;
  };

  const renderItems = (items: PageItem[]) =>
    items.map((item) =>
      item.kind === 'ellipsis' ? (
        <span key={item.key} aria-hidden="true" className="w-5 sm:w-7 text-center text-sm text-brand-secondary-text">
          …
        </span>
      ) : item.page === currentPage ? (
        <span key={item.page} aria-current="page" className={`${currentClass} w-11`}>
          {item.page}
        </span>
      ) : (
        <Link key={item.page} href={hrefOf(item.page)} aria-label={`${item.page}ページ目`} className={`${linkClass} w-11`}>
          {item.page}
        </Link>
      )
    );

  return (
    <div className="flex flex-col items-center gap-3 pt-2">
      <nav aria-label="ページ送り" className="flex items-center gap-1.5 sm:gap-2">
        {hasPrev ? (
          <Link href={hrefOf(currentPage - 1)} rel="prev" aria-label="前のページ" className={`${linkClass} ${stepClass}`}>
            <ChevronLeft className="w-4 h-4" aria-hidden="true" />
            <span className="hidden sm:inline">前へ</span>
          </Link>
        ) : (
          <span aria-disabled="true" className={`${disabledClass} ${stepClass}`}>
            <ChevronLeft className="w-4 h-4" aria-hidden="true" />
            <span className="hidden sm:inline">前へ</span>
          </span>
        )}

        {totalPages !== undefined ? (
          <>
            {/* スマホ幅では前後のページを省き、ボタンが横にはみ出さないようにする */}
            <div className="contents sm:hidden">{renderItems(buildPageItems(currentPage, totalPages, 0))}</div>
            <div className="hidden sm:contents">{renderItems(buildPageItems(currentPage, totalPages, 1))}</div>
          </>
        ) : (
          // 総ページ数が分からないため、現在のページ番号だけを表示する
          <span aria-current="page" className={`${currentClass} w-11`}>
            {currentPage}
          </span>
        )}

        {hasNext ? (
          <Link href={hrefOf(currentPage + 1)} rel="next" aria-label="次のページ" className={`${linkClass} ${stepClass}`}>
            <span className="hidden sm:inline">次へ</span>
            <ChevronRight className="w-4 h-4" aria-hidden="true" />
          </Link>
        ) : (
          <span aria-disabled="true" className={`${disabledClass} ${stepClass}`}>
            <span className="hidden sm:inline">次へ</span>
            <ChevronRight className="w-4 h-4" aria-hidden="true" />
          </span>
        )}
      </nav>
      {totalCount !== undefined && pageSize !== undefined && (currentPage - 1) * pageSize < totalCount && (
        <p className="text-[13px] text-brand-secondary-text">
          {((currentPage - 1) * pageSize + 1).toLocaleString('ja-JP')}〜{Math.min(currentPage * pageSize, totalCount).toLocaleString('ja-JP')}件目 / {totalCount.toLocaleString('ja-JP')}件
        </p>
      )}
    </div>
  );
}
