'use client';

import ReactMarkdown from 'react-markdown';
import remarkBreaks from 'remark-breaks';
import { ShogiBoard } from './ShogiBoard';
import { LinkCard } from './LinkCard';
import { XPostEmbed } from './XPostEmbed';
import { remarkLinkCard, LINK_CARD_ATTRIBUTE, getXPostId } from '@/lib/link-preview';
import { KifPlayer, parseKifuEmbed } from '@/lib/kif-player';

interface NarinoteMarkdownProps {
  content: string;
  className?: string;
  kifuList?: Array<{ name: string; kifuText: string }>;
}

/**
 * NarinoteMarkdown - Molecule Component
 *
 * Markdownコンテンツを一貫したスタイルでレンダリング
 * コードブロック内の将棋盤面（language-bod）も自動的に表示
 */
export function NarinoteMarkdown({ content, className, kifuList = [] }: NarinoteMarkdownProps) {
  return (
    <div className={className}>
      <ReactMarkdown
        remarkPlugins={[remarkBreaks, remarkLinkCard]}
        components={{
          h1: ({ ...props }) => <h1 className="text-3xl font-bold mt-8 mb-4" {...props} />,
          h2: ({ ...props }) => <h2 className="text-2xl font-bold mt-6 mb-3" {...props} />,
          h3: ({ ...props }) => <h3 className="text-xl font-bold mt-4 mb-2" {...props} />,
          p: ({ ...props }) => <p className="mb-4" {...props} />,
          div: ({ node, ...props }) => {
            // URL だけの段落は remarkLinkCard によりリンクカード用の div になる
            const linkCardUrl = node?.properties?.[LINK_CARD_ATTRIBUTE];
            if (typeof linkCardUrl === 'string') {
              const xPostId = getXPostId(linkCardUrl);
              return xPostId ? <XPostEmbed url={linkCardUrl} postId={xPostId} /> : <LinkCard url={linkCardUrl} />;
            }
            return <div {...props} />;
          },
          code: ({ className, children, ...props }) => {
            const isInline = !className;

            // 言語指定が bod の場合は将棋盤面としてレンダリング
            if (className?.includes('language-bod')) {
              return (
                <div className="flex justify-center">
                  <ShogiBoard bodText={String(children)} />
                </div>
              );
            }

            // 言語指定が kifu の場合は棋譜プレイヤーとしてレンダリング
            if (className?.includes('language-kifu')) {
              const { name, moveNumber: defaultMoveNumber } = parseKifuEmbed(String(children));
              const kifu = kifuList.find((k) => k.name === name);
              if (!kifu) {
                return (
                  <div className="p-4 bg-yellow-50 border border-yellow-200 rounded text-yellow-700 text-sm my-4">
                    棋譜「{name}」が見つかりません。
                  </div>
                );
              }
              return (
                <div className="flex justify-center my-4">
                  <KifPlayer kifText={kifu.kifuText} defaultMoveNumber={defaultMoveNumber} />
                </div>
              );
            }

            return isInline ? (
              <code className="bg-gray-100 px-1.5 py-0.5 rounded text-sm font-mono text-pink-600" {...props}>
                {children}
              </code>
            ) : (
              <code className="block bg-gray-900 text-gray-100 p-4 rounded-lg overflow-x-auto font-mono text-sm" {...props}>
                {children}
              </code>
            );
          },
          pre: ({ ...props }) => <pre className="my-4" {...props} />,
          ul: ({ ...props }) => <ul className="list-disc list-inside mb-4 space-y-1" {...props} />,
          ol: ({ ...props }) => <ol className="list-decimal list-inside mb-4 space-y-1" {...props} />,
          blockquote: ({ ...props }) => <blockquote className="border-l-4 border-gray-300 pl-4 italic my-4" {...props} />,
          a: ({ ...props }) => <a className="text-brand-primary hover:underline" {...props} />,
          hr: ({ ...props }) => <hr className="my-6 border-t-2 border-gray-300" {...props} />,
          img: ({ src, alt, ...props }) => {
            // アップロード中の仮 URL など、表示できない src は代替テキストだけを出す
            if (!src) return <span className="text-sm text-gray-400">{alt}</span>;
            // 外部 URL の画像はサイズが不明なため next/image ではなく img を使う
            // eslint-disable-next-line @next/next/no-img-element
            return <img src={src} alt={alt ?? ''} loading="lazy" className="block max-w-full h-auto mx-auto my-4 rounded" {...props} />;
          },
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
}
