import Link from 'next/link';

interface ArticleTagListProps {
  tags: string[];
}

/**
 * ArticleTagList - Molecule Component
 *
 * 記事のタグ一覧をリンク付きで表示するコンポーネント
 */
export function ArticleTagList({ tags }: ArticleTagListProps) {
  if (tags.length === 0) return null;

  return (
    <div className="flex gap-2 flex-wrap pt-6 border-t border-gray-200">
      {tags.map((tag) => (
        <Link
          key={tag}
          href={`/tags/${tag}`}
          className="px-3 py-1 bg-brand-bg-light text-brand-text rounded-full text-sm hover:bg-brand-bg-gradient-to transition-colors"
        >
          #{tag}
        </Link>
      ))}
    </div>
  );
}
