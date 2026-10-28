'use client';

import { useEffect, useRef, useState } from 'react';
import { loadXWidgets } from '@/lib/link-preview';
import { useDebouncedValue } from '@/lib/hooks';
import { LinkCard } from './LinkCard';

// エディタのプレビューで URL を入力中に、途中までの投稿 ID で描画しないための待ち時間
const EMBED_DEBOUNCE_MS = 500;

interface XPostEmbedProps {
  url: string;
  postId: string;
}

/**
 * XPostEmbed - Molecule Component
 *
 * X の投稿を公式の埋め込みで表示する。
 * 描画されるまでの間と、削除済み・非公開などで描画できない場合は LinkCard を表示する
 */
export function XPostEmbed({ url, postId }: XPostEmbedProps) {
  const debouncedPostId = useDebouncedValue(postId, EMBED_DEBOUNCE_MS);
  const containerRef = useRef<HTMLDivElement>(null);
  const [embedded, setEmbedded] = useState<{ postId: string; succeeded: boolean }>();

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    // 古い投稿の描画が後から完了しても混ざらないよう、描画ごとに専用の要素を用意する
    const target = document.createElement('div');
    container.appendChild(target);
    let cancelled = false;

    loadXWidgets()
      .then((twttr) => twttr.widgets.createTweet(debouncedPostId, target, { dnt: true, lang: 'ja', align: 'center' }))
      .then((element) => {
        if (!cancelled) setEmbedded({ postId: debouncedPostId, succeeded: !!element });
      })
      .catch(() => {
        if (!cancelled) setEmbedded({ postId: debouncedPostId, succeeded: false });
      });

    return () => {
      cancelled = true;
      target.remove();
    };
  }, [debouncedPostId]);

  const isEmbedded = embedded?.postId === postId && embedded.succeeded;

  return (
    <div className="my-4">
      <div ref={containerRef} />
      {!isEmbedded && <LinkCard url={url} />}
    </div>
  );
}
