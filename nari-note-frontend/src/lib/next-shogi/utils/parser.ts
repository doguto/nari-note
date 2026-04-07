import type { BoardState, CapturedPiece, ParsedBoard } from '../types';
import { PIECE_MAP, KANJI_NUMBER_MAP, EMPTY_SQUARE } from '../constants';

/**
 * 漢数字を数値に変換する
 * 例: "三" → 3, "十三" → 13, "二十" → 20
 */
function parseJapaneseNumber(text: string): number {
  if (!text) return 1; // 数字がない場合は1

  let result = 0;
  let hasJuu = false;
  let juuMultiplier = 0;

  for (let i = 0; i < text.length; i++) {
    const char = text[i];

    if (char === '十') {
      hasJuu = true;
      // 「十」の前に数字があればそれを10倍、なければ10
      if (juuMultiplier > 0) {
        result += juuMultiplier * 10;
        juuMultiplier = 0;
      } else {
        result += 10;
      }
    } else if (char in KANJI_NUMBER_MAP) {
      const num = KANJI_NUMBER_MAP[char];
      if (hasJuu) {
        // 「十」の後の数字
        result += num;
        hasJuu = false;
      } else {
        juuMultiplier = num;
      }
    }
  }

  // 最後に残った数字を加算
  if (juuMultiplier > 0 && !hasJuu) {
    result += juuMultiplier;
  }

  return result || 1;
}

/**
 * 持駒文字列をパースする
 * 例: "飛　金三　銀三　桂二" → [{type: '飛', count: 1}, {type: '金', count: 3}, ...]
 */
export function parseCapturedPieces(text: string): CapturedPiece[] {
  if (!text || text === 'なし') {
    return [];
  }

  const pieces: CapturedPiece[] = [];
  // 全角空白または半角空白で分割
  const parts = text.split(/[\s　]+/).filter(Boolean);

  for (const part of parts) {
    // 駒種と数を抽出（例: "金三" → 駒種="金", 数="三"）
    const match = part.match(/^(v?[玉王飛角金銀桂香歩龍馬とと]|成[銀桂香])(.*)$/);
    if (match) {
      const pieceType = match[1];
      const countStr = match[2];
      const count = countStr ? parseJapaneseNumber(countStr) : 1;

      // v接頭辞を除去して駒種を取得
      const normalizedType = pieceType.replace(/^v/, '') as CapturedPiece['type'];

      pieces.push({
        type: normalizedType,
        count,
      });
    }
  }

  return pieces;
}

/**
 * 盤面の1行をパースする
 * 例: "| ・ ・ ・ ・ 馬 ・ ・v桂 ・|一"
 */
function parseBoardRow(line: string, rowIndex: number, board: BoardState): void {
  // "|"で囲まれた部分を抽出
  const match = line.match(/\|(.+)\|/);
  if (!match) return;

  const content = match[1];
  let colIndex = 0;

  // 2文字ずつ処理（駒は最大2文字: "v" + 駒種）
  for (let i = 0; i < content.length && colIndex < 9; ) {
    // 空白をスキップ
    if (content[i] === ' ' || content[i] === '　') {
      i++;
      continue;
    }

    // 2文字読み取り（v接頭辞の可能性を考慮）
    let pieceStr = content.substring(i, i + 2).trim();

    // 1文字の場合（v接頭辞なし）
    if (pieceStr.length === 1 || !pieceStr.startsWith('v')) {
      pieceStr = content[i];
      i++;
    } else {
      i += 2;
    }

    // 空白をスキップ
    if (!pieceStr || pieceStr === ' ' || pieceStr === '　') {
      continue;
    }

    // 空マスの場合
    if (pieceStr === EMPTY_SQUARE) {
      board[rowIndex][colIndex] = null;
      colIndex++;
      continue;
    }

    // 成駒の処理（"成銀"、"成桂"、"成香"）
    if (pieceStr === '成' && i < content.length) {
      const nextChar = content[i];
      if (nextChar === '銀' || nextChar === '桂' || nextChar === '香') {
        pieceStr = '成' + nextChar;
        i++;
      }
    } else if (pieceStr.startsWith('v') && pieceStr.length === 2 && pieceStr[1] === '成') {
      // v成の場合、次の文字を読む
      if (i < content.length) {
        const nextChar = content[i];
        if (nextChar === '銀' || nextChar === '桂' || nextChar === '香') {
          pieceStr = 'v成' + nextChar;
          i++;
        }
      }
    }

    // 駒をマップから取得
    const piece = PIECE_MAP[pieceStr];
    if (piece) {
      board[rowIndex][colIndex] = piece;
      colIndex++;
    } else {
      // 認識できない駒の場合は空マスとして扱う
      board[rowIndex][colIndex] = null;
      colIndex++;
    }
  }
}

/**
 * BOD形式のテキストをパースする
 */
export function parseBOD(bodText: string): ParsedBoard {
  const lines = bodText.split('\n').map((line) => line.trim());

  // 9x9の盤面を初期化
  const board: BoardState = Array.from({ length: 9 }, () =>
    Array.from({ length: 9 }, () => null)
  );

  const result: ParsedBoard = {
    board,
    captured: {
      sente: [],
      gote: [],
    },
  };

  let boardStartIndex = -1;

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];

    // 後手の持駒
    if (line.startsWith('後手の持駒：')) {
      const capturedText = line.replace('後手の持駒：', '').trim();
      result.captured.gote = parseCapturedPieces(capturedText);
    }
    // 先手の持駒
    else if (line.startsWith('先手の持駒：')) {
      const capturedText = line.replace('先手の持駒：', '').trim();
      result.captured.sente = parseCapturedPieces(capturedText);
    }
    // 盤面の開始行（+で始まり-を含む行）
    else if (line.startsWith('+') && line.includes('-') && boardStartIndex === -1) {
      boardStartIndex = i + 1; // 次の行から盤面
    }
    // 盤面の行
    else if (boardStartIndex !== -1 && i >= boardStartIndex && i < boardStartIndex + 9) {
      const rowIndex = i - boardStartIndex;
      parseBoardRow(line, rowIndex, board);
    }
    // プレイヤー名
    else if (line.startsWith('先手：')) {
      result.sentePlayer = line.replace('先手：', '').trim();
    } else if (line.startsWith('後手：')) {
      result.gotePlayer = line.replace('後手：', '').trim();
    }
  }

  return result;
}
