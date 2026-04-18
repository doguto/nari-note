import type { BoardState, CapturedPiece, ParsedBoard } from '../types';
import { PIECE_MAP, KANJI_NUMBER_MAP, EMPTY_SQUARE } from '../constants';

function parseJapaneseNumber(text: string): number {
  if (!text) return 1;

  let result = 0;
  let hasJuu = false;
  let juuMultiplier = 0;

  for (let i = 0; i < text.length; i++) {
    const char = text[i];
    if (char === '十') {
      hasJuu = true;
      if (juuMultiplier > 0) {
        result += juuMultiplier * 10;
        juuMultiplier = 0;
      } else {
        result += 10;
      }
    } else if (char in KANJI_NUMBER_MAP) {
      const num = KANJI_NUMBER_MAP[char];
      if (hasJuu) {
        result += num;
        hasJuu = false;
      } else {
        juuMultiplier = num;
      }
    }
  }

  if (juuMultiplier > 0 && !hasJuu) result += juuMultiplier;
  return result || 1;
}

export function parseCapturedPieces(text: string): CapturedPiece[] {
  if (!text || text === 'なし') return [];

  const pieces: CapturedPiece[] = [];
  const parts = text.split(/[\s　]+/).filter(Boolean);

  for (const part of parts) {
    const match = part.match(/^(v?[玉王飛角金銀桂香歩龍馬とと]|成[銀桂香])(.*)$/);
    if (match) {
      const pieceType = match[1];
      const count = match[2] ? parseJapaneseNumber(match[2]) : 1;
      const normalizedType = pieceType.replace(/^v/, '') as CapturedPiece['type'];
      pieces.push({ type: normalizedType, count });
    }
  }

  return pieces;
}

function parseBoardRow(line: string, rowIndex: number, board: BoardState): void {
  const match = line.match(/\|(.+)\|/);
  if (!match) return;

  const content = match[1];
  let colIndex = 0;

  for (let i = 0; i < content.length && colIndex < 9; ) {
    if (content[i] === ' ' || content[i] === '　') { i++; continue; }

    let pieceStr = content.substring(i, i + 2).trim();
    if (pieceStr.length === 1 || !pieceStr.startsWith('v')) {
      pieceStr = content[i];
      i++;
    } else {
      i += 2;
    }

    if (!pieceStr || pieceStr === ' ' || pieceStr === '　') continue;

    if (pieceStr === EMPTY_SQUARE) {
      board[rowIndex][colIndex++] = null;
      continue;
    }

    if (pieceStr === '成' && i < content.length) {
      const next = content[i];
      if (next === '銀' || next === '桂' || next === '香') { pieceStr = '成' + next; i++; }
    } else if (pieceStr.startsWith('v') && pieceStr.length === 2 && pieceStr[1] === '成') {
      if (i < content.length) {
        const next = content[i];
        if (next === '銀' || next === '桂' || next === '香') { pieceStr = 'v成' + next; i++; }
      }
    }

    const piece = PIECE_MAP[pieceStr];
    board[rowIndex][colIndex++] = piece ?? null;
  }
}

export function parseBOD(bodText: string): ParsedBoard {
  const lines = bodText.split('\n').map((l) => l.trim());
  const board: BoardState = Array.from({ length: 9 }, () => Array.from({ length: 9 }, () => null));
  const result: ParsedBoard = { board, captured: { sente: [], gote: [] } };

  let boardStartIndex = -1;

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    if (line.startsWith('後手の持駒：')) {
      result.captured.gote = parseCapturedPieces(line.replace('後手の持駒：', '').trim());
    } else if (line.startsWith('先手の持駒：')) {
      result.captured.sente = parseCapturedPieces(line.replace('先手の持駒：', '').trim());
    } else if (line.startsWith('+') && line.includes('-') && boardStartIndex === -1) {
      boardStartIndex = i + 1;
    } else if (boardStartIndex !== -1 && i >= boardStartIndex && i < boardStartIndex + 9) {
      parseBoardRow(line, i - boardStartIndex, board);
    } else if (line.startsWith('先手：')) {
      result.sentePlayer = line.replace('先手：', '').trim();
    } else if (line.startsWith('後手：')) {
      result.gotePlayer = line.replace('後手：', '').trim();
    }
  }

  return result;
}
