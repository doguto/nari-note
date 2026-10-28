'use client';

import { useState } from 'react';
import { useLinkPreview } from '@/lib/link-preview';
import { useDebouncedValue } from '@/lib/hooks';

// エディタのプレビューで URL を入力中に、途中までの URL で取得しないための待ち時間
const FETCH_DEBOUNCE_MS = 500;

interface LinkCardProps {
  url: string;
}

function getHostname(url: string): string {
  try {
    return new URL(url).hostname;
  } catch {
    return url;
  }
}

/**
 * LinkCard - Molecule Component
 *
 * URL の OGP 情報（タイトル・説明・画像）をカード形式で表示する
 */
export function LinkCard({ url }: LinkCardProps) {
  const debouncedUrl = useDebouncedValue(url, FETCH_DEBOUNCE_MS);
  const { data } = useLinkPreview(debouncedUrl);
  const [failedImageUrl, setFailedImageUrl] = useState<string>();

  const preview = debouncedUrl === url ? data : undefined;
  const imageUrl = preview?.imageUrl !== failedImageUrl ? preview?.imageUrl : undefined;

  // URL 自体が画像の場合は、Markdown の画像記法と同じ見た目で表示する
  if (preview?.isImage) {
    return (
      <a href={url} target="_blank" rel="noopener noreferrer">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={url} alt="" loading="lazy" className="block max-w-full h-auto mx-auto my-4 rounded" />
      </a>
    );
  }

  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className="my-4 flex h-32 overflow-hidden rounded-lg border border-gray-200 bg-white transition-colors hover:bg-gray-50"
    >
      <div className="flex min-w-0 flex-1 flex-col justify-center gap-1 px-4 py-3">
        <div className="line-clamp-2 font-bold text-gray-900 break-all">
          {/* 取得中はホスト名、OGP が取れなかった場合は URL をそのまま出す */}
          {preview?.title ?? (preview ? url : getHostname(url))}
        </div>
        {preview?.description && (
          <div className="line-clamp-2 text-sm text-gray-500">{preview.description}</div>
        )}
        <div className="truncate text-xs text-gray-400">{preview?.siteName ?? getHostname(url)}</div>
      </div>
      {imageUrl && (
        // 外部サイトの画像はサイズや配信元が不定のため next/image ではなく img を使う
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={imageUrl}
          alt=""
          loading="lazy"
          onError={() => setFailedImageUrl(imageUrl)}
          className="h-full w-28 shrink-0 object-cover sm:w-48"
        />
      )}
    </a>
  );
}
