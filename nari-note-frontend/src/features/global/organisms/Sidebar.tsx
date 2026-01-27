'use client';

import Link from 'next/link';
import { useMemo } from 'react';
import { useGetTags } from '@/lib/api/hooks';

/**
 * サイドバーコンポーネント
 * 
 * トレンドタグと注目の記事を表示します。
 */
export function Sidebar() {
  const { data: tagsData, isLoading, isError } = useGetTags();

  // タグをarticleCountの降順でソートして上位5個を取得
  const topTags = useMemo(() => {
    return tagsData?.tags
      ? [...tagsData.tags]
          .filter((tag) => tag.name) // nameが存在するタグのみをフィルタ
          .sort((a, b) => (b.articleCount || 0) - (a.articleCount || 0))
          .slice(0, 5)
      : [];
  }, [tagsData]);

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
          {!isLoading && !isError && topTags.length === 0 && (
            <div className="text-sm text-gray-300">タグがありません</div>
          )}
          {!isLoading && !isError && topTags.map((tag) => (
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

      {/* 注目の記事セクション */}
      <div className="bg-brand-bg-light rounded-lg p-4 border border-brand-border">
        <h3 className="text-lg font-bold text-brand-text mb-4" style={{ fontFamily: 'serif' }}>
          注目の記事
        </h3>
        <div className="space-y-3">
          {[
            { id: 1, image: '👤', label: 'プログラミング入門', count: '471いいね' },
            { id: 2, image: '🎮', label: 'ゲーム開発', count: '356いいね' },
            { id: 3, image: '📱', label: 'Web開発', count: '289いいね' }
          ].map((item) => (
            <Link 
              key={item.id} 
              href={`/articles/${item.id}`} 
              className="flex items-center gap-2 hover:opacity-80 transition-opacity"
            >
              <div className="w-12 h-12 bg-brand-bg-gradient-to rounded flex items-center justify-center text-xl flex-shrink-0">
                {item.image}
              </div>
              <div className="flex-1 text-sm">
                <div className="text-brand-secondary-text">{item.label}</div>
                <div className="text-gray-400">{item.count}</div>
              </div>
            </Link>
          ))}
        </div>
      </div>

      {/* フッター */}
      <div className="mt-6 text-center text-sm text-gray-500">
        © 2024 なりノート
      </div>
    </aside>
  );
}
