import Link from 'next/link';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  basePath: string;
}

/**
 * Pagination - Molecule Component
 *
 * `?page=N` のクエリでページを切り替えるページ送り
 */
export function Pagination({ currentPage, totalPages, basePath }: PaginationProps) {
  if (totalPages <= 1) return null;

  const hrefOf = (page: number) => (page === 1 ? basePath : `${basePath}?page=${page}`);
  const linkClass = 'px-4 py-2 rounded-lg border border-brand-border text-brand-text hover:bg-brand-bg-light';
  const disabledClass = 'px-4 py-2 rounded-lg border border-brand-border text-gray-300';

  return (
    <nav aria-label="ページ送り" className="flex items-center justify-center gap-4">
      {currentPage > 1 ? (
        <Link href={hrefOf(currentPage - 1)} className={linkClass} rel="prev">
          前へ
        </Link>
      ) : (
        <span className={disabledClass} aria-disabled="true">前へ</span>
      )}
      <span className="text-gray-600">
        {currentPage} / {totalPages}
      </span>
      {currentPage < totalPages ? (
        <Link href={hrefOf(currentPage + 1)} className={linkClass} rel="next">
          次へ
        </Link>
      ) : (
        <span className={disabledClass} aria-disabled="true">次へ</span>
      )}
    </nav>
  );
}
