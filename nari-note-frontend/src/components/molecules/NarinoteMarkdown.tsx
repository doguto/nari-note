import ReactMarkdown from 'react-markdown';
import { ShogiBoard } from './ShogiBoard';

interface NarinoteMarkdownProps {
  content: string;
  className?: string;
}

/**
 * NarinoteMarkdown - Molecule Component
 *
 * Markdownコンテンツを一貫したスタイルでレンダリング
 * コードブロック内の将棋盤面（language-bod）も自動的に表示
 */
export function NarinoteMarkdown({ content, className }: NarinoteMarkdownProps) {
  return (
    <div className={className}>
      <ReactMarkdown
        components={{
          h1: ({ ...props }) => <h1 className="text-3xl font-bold mt-8 mb-4" {...props} />,
          h2: ({ ...props }) => <h2 className="text-2xl font-bold mt-6 mb-3" {...props} />,
          h3: ({ ...props }) => <h3 className="text-xl font-bold mt-4 mb-2" {...props} />,
          p: ({ ...props }) => <p className="mb-4" {...props} />,
          code: ({ className, children, ...props }) => {
            const isInline = !className;

            // 言語指定が bod の場合は将棋盤面としてレンダリング
            if (className?.includes('language-bod')) {
              return <ShogiBoard bodText={String(children)} />;
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
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
}
