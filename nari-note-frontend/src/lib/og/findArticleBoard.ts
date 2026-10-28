import { getBoardAtMove, parseBOD, parseKif, parseKifuEmbed } from '@/lib/kif-player/utils';
import type { ParsedBoard } from '@/lib/kif-player/types';
import type { ArticleDto } from '@/lib/api/types';

const EMBED_BLOCK_PATTERN = /^```(kifu|bod)[ \t]*\n([\s\S]*?)\n```[ \t]*$/gm;

/**
 * OG 画像に描く局面を本文の埋め込みから決める。盤面埋め込みを棋譜埋め込みより優先し、それぞれ最初のものを使う。
 * 解析できない埋め込みは飛ばす
 */
export function findArticleBoard(article: Pick<ArticleDto, 'body' | 'kifus'>): ParsedBoard | null {
  const blocks = Array.from(article.body.matchAll(EMBED_BLOCK_PATTERN), (m) => ({ lang: m[1], content: m[2] }));

  for (const block of blocks.filter((b) => b.lang === 'bod')) {
    const board = tryParse(() => parseBOD(block.content));
    if (board && hasPiece(board)) return board;
  }

  for (const block of blocks.filter((b) => b.lang === 'kifu')) {
    const { name, moveNumber } = parseKifuEmbed(block.content);
    const kifu = article.kifus.find((k) => k.name === name);
    if (!kifu) continue;
    const board = tryParse(() => getBoardAtMove(parseKif(kifu.kifuText), moveNumber));
    if (board) return board;
  }

  return null;
}

function tryParse(parse: () => ParsedBoard): ParsedBoard | null {
  try {
    return parse();
  } catch {
    return null;
  }
}

function hasPiece(board: ParsedBoard): boolean {
  return board.board.some((row) => row.some((piece) => piece !== null));
}
