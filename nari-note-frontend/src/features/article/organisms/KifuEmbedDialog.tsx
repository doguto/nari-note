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
import { KifPlayer } from '@/lib/kif-player';
import { parseKif } from '@/lib/kif-player/utils';
import type { KifMove } from '@/lib/kif-player/types';
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
  kifuList: KifuItem[];
}

export function KifuEmbedDialog({
  open,
  onOpenChange,
  onConfirm,
  kifuList,
}: KifuEmbedDialogProps) {
  const [selectedIndex, setSelectedIndex] = useState<number>(0);
  const [move, setMove] = useState<number>(0);

  useEffect(() => {
    if (open) {
      setSelectedIndex(0);
      setMove(0);
    }
  }, [open]);

  const selectedKifu = kifuList[selectedIndex];

  const moveOptions = useMemo(() => {
    if (!selectedKifu?.text.trim()) return [];
    try {
      return parseKif(selectedKifu.text).moves;
    } catch {
      return [];
    }
  }, [selectedKifu]);

  const handleKifuChange = (index: number) => {
    setSelectedIndex(index);
    setMove(0);
  };

  const handleConfirm = () => {
    if (!selectedKifu) return;
    onConfirm(selectedKifu, move);
    onOpenChange(false);
  };

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
                  onChange={(e) => setMove(Number(e.target.value))}
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
                <div className="flex justify-center border rounded-lg p-4 bg-gray-50">
                  <KifPlayer
                    kifText={selectedKifu.text}
                    moveNumber={move}
                    onMoveChange={setMove}
                    size="sm"
                  />
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
            disabled={!selectedKifu}
            className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
          >
            埋め込む
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
