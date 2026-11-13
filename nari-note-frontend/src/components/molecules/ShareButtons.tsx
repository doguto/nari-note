'use client';

import { useState } from 'react';
import { Check, Link as LinkIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { XLogo } from '@/components/ui';

const SERVICE_HASHTAG = 'なりノート';
const MAX_TAG_HASHTAGS = 3;

interface ShareButtonsProps {
  title: string;
  path: string;
  tags?: string[];
}

/**
 * X 投稿画面の URL を組み立てる
 * 本文は「タイトル / URL / #なりノート / 記事のタグ（最大3件）」を改行区切りで並べる
 */
function buildXShareUrl(title: string, url: string, tags: string[] = []): string {
  const lines = [title, url, `#${SERVICE_HASHTAG}`];
  const tagHashtags = tags.slice(0, MAX_TAG_HASHTAGS).map((tag) => `#${tag}`).join(' ');
  if (tagHashtags) {
    lines.push(tagHashtags);
  }
  return `https://x.com/intent/post?text=${encodeURIComponent(lines.join('\n'))}`;
}

/**
 * ShareButtons - Molecule Component
 *
 * 「X でシェア」「リンクをコピー」ボタンを表示する
 */
export function ShareButtons({ title, path, tags = [] }: ShareButtonsProps) {
  const [copied, setCopied] = useState(false);

  // 開発環境などでも開いているサイトの URL になるよう、クリック時に origin を取得する
  const getUrl = () => `${window.location.origin}${path}`;

  const handleShareToX = () => {
    window.open(buildXShareUrl(title, getUrl(), tags), '_blank', 'noopener,noreferrer');
  };

  const handleCopyLink = async () => {
    try {
      await navigator.clipboard.writeText(getUrl());
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (error) {
      console.error('リンクのコピーに失敗しました:', error);
    }
  };

  return (
    <div className="flex flex-wrap items-center gap-2">
      <Button
        type="button"
        size="sm"
        onClick={handleShareToX}
        className="bg-black text-white hover:bg-black/80"
      >
        <XLogo />
        でシェア
      </Button>
      <Button type="button" size="sm" variant="outline" onClick={handleCopyLink}>
        {copied ? <Check /> : <LinkIcon />}
        {copied ? 'コピーしました' : 'リンクをコピー'}
      </Button>
    </div>
  );
}
