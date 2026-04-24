'use client';

import { useState, useEffect, useMemo } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { KifPlayer, Board, CapturedPieces, useFreePlayRecorder, getBoardAtMove, parseKif } from '@/lib/kif-player';
import type { KifMove, PieceOwner } from '@/lib/kif-player/types';
import type { KifuItem } from '../types/kifu';

const COL_LABEL = ['', '１', '２', '３', '４', '５', '６', '７', '８', '９'];
const ROW_LABEL = ['', '一', '二', '三', '四', '五', '六', '七', '八', '九'];

function formatMove(move: KifMove): string {
  const square = move.isSameSquare ? '同　' : `${COL_LABEL[move.toCol]}${ROW_LABEL[move.toRow]}`;
  const suffix = move.isPromote ? '成' : move.isDrop ? '打' : '';
  return `${move.moveNumber}: ${square}${move.piece}${suffix}`;
}

interface KifuEmbedDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (kifu: KifuItem, move: number) => void;
  onConfirmBOD?: (bod: string) => void;
  onSaveAsKifu?: (name: string, kifText: string, totalMoves: number) => void;
  kifuList: KifuItem[];
  kifuCount?: number;
  initialKifuName?: string;
  initialMove?: number;
}

export function KifuEmbedDialog({
  open,
  onOpenChange,
  onConfirm,
  onConfirmBOD,
  onSaveAsKifu,
  kifuList,
  kifuCount = 0,
  initialKifuName,
  initialMove,
}: KifuEmbedDialogProps) {
  const [selectedIndex, setSelectedIndex] = useState<number>(0);
  const [move, setMove] = useState<number>(0);
  const [freePlayMode, setFreePlayMode] = useState(false);
  const [embedType, setEmbedType] = useState<'bod' | 'kifu'>('bod');
  const [newKifuName, setNewKifuName] = useState('');

  const recorder = useFreePlayRecorder();

  useEffect(() => {
    if (open) {
      const idx = initialKifuName
        ? Math.max(0, kifuList.findIndex((k) => k.name === initialKifuName))
        : 0;
      setSelectedIndex(idx);
      setMove(initialMove ?? 0);
      setFreePlayMode(false);
      setEmbedType('bod');
      setNewKifuName('');
    }
  }, [open]); // eslint-disable-line react-hooks/exhaustive-deps

  const selectedKifu = kifuList[selectedIndex];

  const parsedGame = useMemo(() => {
    if (!selectedKifu?.text.trim()) return null;
    try {
      return parseKif(selectedKifu.text);
    } catch {
      return null;
    }
  }, [selectedKifu]);

  const moveOptions = parsedGame?.moves ?? [];

  const handleKifuChange = (index: number) => {
    setSelectedIndex(index);
    setMove(0);
    setFreePlayMode(false);
  };

  const handleMoveChange = (nextMove: number) => {
    setMove(nextMove);
    setFreePlayMode(false);
  };

  const handleEnterFreePlay = () => {
    if (!parsedGame) return;
    const parsed = getBoardAtMove(parsedGame, move);
    const initialTurn: PieceOwner = move % 2 === 0 ? 'sente' : 'gote';
    // 初期は BOD モード（手番強制なし）
    recorder.initializeBoard(parsed.board, parsed.captured.sente, parsed.captured.gote, initialTurn, false);
    setFreePlayMode(true);
    setEmbedType('bod');
    setNewKifuName(`棋譜${kifuCount + 1}`);
  };

  const handleEmbedTypeChange = (type: 'bod' | 'kifu') => {
    setEmbedType(type);
    recorder.setTurnEnforced(type === 'kifu');
  };

  const handleConfirm = () => {
    if (!selectedKifu) return;
    if (freePlayMode && onSaveAsKifu) {
      const name = newKifuName.trim() || `棋譜${kifuCount + 1}`;
      if (embedType === 'kifu') {
        const kifText = recorder.generateKIF(selectedKifu.text, move);
        const totalMoves = move + recorder.moveHistory.length;
        onSaveAsKifu(name, kifText, totalMoves);
      } else {
        onSaveAsKifu(name, recorder.generateCurrentBOD(), 0);
      }
    } else if (!freePlayMode) {
      onConfirm(selectedKifu, move);
    }
    onOpenChange(false);
  };

  const selectedBoardCell = recorder.selected?.from === 'board'
    ? { row: recorder.selected.row, col: recorder.selected.col }
    : null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-full max-w-2xl">
        <DialogHeader>
          <DialogTitle>棋譜プレイヤーを埋め込む</DialogTitle>
          <DialogDescription>
            埋め込む棋譜と初期手数を選択してください
          </DialogDescription>
        </DialogHeader>

        <div className="overflow-y-auto max-h-[60vh] pr-1">
          {kifuList.length === 0 ? (
            <div className="py-8 text-center text-muted-foreground text-sm">
              棋譜が登録されていません。先に棋譜を追加してください。
            </div>
          ) : (
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="embed-kifu-select">棋譜を選択</Label>
                <select
                  id="embed-kifu-select"
                  value={selectedIndex}
                  onChange={(e) => handleKifuChange(Number(e.target.value))}
                  className="w-full border border-input rounded-md px-3 py-2 text-sm bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  {kifuList.map((kifu, i) => (
                    <option key={i} value={i}>
                      {kifu.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="embed-move-select">初期手数を選択</Label>
                <select
                  id="embed-move-select"
                  value={move}
                  onChange={(e) => handleMoveChange(Number(e.target.value))}
                  className="w-full border border-input rounded-md px-3 py-2 text-sm bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  <option value={0}>0: 初期配置</option>
                  {moveOptions.map((m) => (
                    <option key={m.moveNumber} value={m.moveNumber}>
                      {formatMove(m)}
                    </option>
                  ))}
                </select>
              </div>

              {selectedKifu?.text.trim() && (
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground flex items-center gap-2">
                      {freePlayMode
                        ? `${recorder.moveHistory.length}手 記録済み`
                        : 'プレビュー'}
                      {freePlayMode && embedType === 'kifu' && (
                        <span className={
                          recorder.currentTurn === 'sente'
                            ? 'text-blue-600 font-medium'
                            : 'text-red-500 font-medium'
                        }>
                          {recorder.currentTurn === 'sente' ? '先手番' : '後手番'}
                        </span>
                      )}
                    </span>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={freePlayMode ? () => setFreePlayMode(false) : handleEnterFreePlay}
                    >
                      {freePlayMode ? 'KIF再生に戻す' : '自由に動かす'}
                    </Button>
                  </div>

                  {freePlayMode && (
                    <div className="space-y-2">
                      <div className="flex gap-4">
                        <label className="flex items-center gap-2 cursor-pointer text-sm">
                          <input
                            type="radio"
                            name="embedType"
                            value="bod"
                            checked={embedType === 'bod'}
                            onChange={() => handleEmbedTypeChange('bod')}
                          />
                          単一の盤面として埋め込む
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer text-sm">
                          <input
                            type="radio"
                            name="embedType"
                            value="kifu"
                            checked={embedType === 'kifu'}
                            onChange={() => handleEmbedTypeChange('kifu')}
                          />
                          棋譜として保存して埋め込む
                        </label>
                      </div>
                      {embedType === 'kifu' && (
                        <Input
                          value={newKifuName}
                          onChange={(e) => setNewKifuName(e.target.value)}
                          placeholder="棋譜名を入力"
                          className="text-sm"
                        />
                      )}
                    </div>
                  )}

                  <div className="flex justify-center border rounded-lg p-4 bg-gray-50">
                    {freePlayMode ? (
                      <div className="inline-flex flex-col items-center gap-2">
                        <CapturedPieces
                          pieces={recorder.goteCaptured}
                          owner="gote"
                          size="sm"
                          onPieceClick={recorder.handleHandClick}
                          isSelected={recorder.isSelectedHand}
                          onHandZoneClick={recorder.handleHandZoneClick}
                        />
                        <Board
                          board={recorder.board}
                          size="sm"
                          selectedCell={selectedBoardCell}
                          onCellClick={recorder.handleBoardClick}
                        />
                        <CapturedPieces
                          pieces={recorder.senteCaptured}
                          owner="sente"
                          size="sm"
                          onPieceClick={recorder.handleHandClick}
                          isSelected={recorder.isSelectedHand}
                          onHandZoneClick={recorder.handleHandZoneClick}
                        />
                        {recorder.pendingPromotion && (
                          <div className="flex items-center gap-3 mt-1 p-2 bg-yellow-50 border border-yellow-200 rounded text-sm">
                            <span>{recorder.pendingPromotion.prePiece}を成りますか？</span>
                            <Button type="button" size="sm" onClick={() => recorder.confirmPromotion(true)}>
                              成る
                            </Button>
                            <Button type="button" size="sm" variant="outline" onClick={() => recorder.confirmPromotion(false)}>
                              成らない
                            </Button>
                          </div>
                        )}
                      </div>
                    ) : (
                      <KifPlayer
                        kifText={selectedKifu.text}
                        moveNumber={move}
                        onMoveChange={setMove}
                        size="sm"
                      />
                    )}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            キャンセル
          </Button>
          <Button
            type="button"
            onClick={handleConfirm}
            disabled={!selectedKifu || !!recorder.pendingPromotion}
            className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
          >
            埋め込む
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
