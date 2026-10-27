'use client';

import { useState } from 'react';
import { Check, Link as LinkIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';

const SERVICE_HASHTAG = 'なりノート';
const MAX_TAG_HASHTAGS = 3;

interface ShareButtonsProps {
  title: string;
  path: string;
  tags?: string[];
}

/**
 * X 投稿画面の URL を組み立てる
 * 本文は「タイトル + #なりノート + 記事のタグ（最大3件）」
 */
function buildXShareUrl(title: string, url: string, tags: string[] = []): string {
  const hashtags = [SERVICE_HASHTAG, ...tags.slice(0, MAX_TAG_HASHTAGS)].map((tag) => `#${tag}`).join(' ');
  const text = encodeURIComponent(`${title}\n${hashtags}`);
  return `https://x.com/intent/post?text=${text}&url=${encodeURIComponent(url)}`;
}

function XLogo() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" fill="currentColor">
      <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
    </svg>
  );
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
    <div className="flex items-center gap-2">
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
