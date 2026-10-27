export interface KifuEmbed {
  name: string;
  moveNumber: number;
}

/**
 * kifu 埋め込みブロックの中身（`棋譜名_手数`）を分解する
 * 末尾が数値でない場合は全体を棋譜名、手数 0 として扱う
 */
export function parseKifuEmbed(text: string): KifuEmbed {
  const raw = text.trim();
  const lastUnderscore = raw.lastIndexOf('_');
  if (lastUnderscore !== -1) {
    const possibleMove = parseInt(raw.substring(lastUnderscore + 1));
    if (!isNaN(possibleMove)) {
      return { name: raw.substring(0, lastUnderscore), moveNumber: possibleMove };
    }
  }
  return { name: raw, moveNumber: 0 };
}
