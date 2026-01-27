'use client';

import Link from 'next/link';
import { useGetPopularTags } from '@/lib/api/hooks';

/**
 * サイドバーコンポーネント
 * 
 * トレンドタグと注目の記事を表示します。
 */
export function Sidebar() {
  const { data: tagsData, isLoading, isError } = useGetPopularTags();

  return (
    <aside className="w-80 hidden lg:block">
      {/* トレンドセクション */}
      <div className="bg-brand-text rounded-lg p-4 text-white mb-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-bold" style={{ fontFamily: 'serif' }}>
            トレンド
          </h3>
          <span className="text-2xl">🔥</span>
        </div>
        <div className="space-y-2">
          {isLoading && (
            <div className="text-sm text-gray-300">読み込み中...</div>
          )}
          {isError && (
            <div className="text-sm text-gray-300">タグの取得に失敗しました</div>
          )}
          {!isLoading && !isError && tagsData?.tags?.length === 0 && (
            <div className="text-sm text-gray-300">タグがありません</div>
          )}
          {!isLoading && !isError && tagsData?.tags?.map((tag) => (
            <Link 
              key={tag.name}
              href={`/tags/${tag.name}`}
              className="block text-sm hover:text-brand-primary cursor-pointer transition-colors"
            >
              #{tag.name}
            </Link>
          ))}
        </div>
      </div>
    </aside>
  );
}
