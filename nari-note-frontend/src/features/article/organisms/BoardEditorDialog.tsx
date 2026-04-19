'use client';

import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { useBoardEditor } from '@/lib/kif-player';
import { COLUMN_LABELS, ROW_LABELS } from '@/lib/kif-player/constants';
import type { CapturedPiece, PieceOwner, PieceType } from '@/lib/kif-player/types';

interface BoardEditorDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (bod: string) => void;
}

export function BoardEditorDialog({ open, onOpenChange, onConfirm }: BoardEditorDialogProps) {
  const {
    board,
    senteCaptured,
    goteCaptured,
    selected,
    handleBoardClick,
    handleHandClick,
    handleHandZoneClick,
    resetBoard,
    clearBoard,
    isSelectedBoard,
    isSelectedHand,
    generateCurrentBOD,
  } = useBoardEditor();

  const handleConfirm = () => {
    onConfirm(generateCurrentBOD());
    onOpenChange(false);
    resetBoard();
  };

  const handleClose = () => {
    onOpenChange(false);
    resetBoard();
  };

  const HandPanel = ({
    owner,
    pieces,
    label,
  }: {
    owner: PieceOwner;
    pieces: CapturedPiece[];
    label: string;
  }) => (
    <div
      className={cn(
        'border rounded-lg p-2 min-h-[44px] cursor-pointer transition-colors select-none',
        selected?.from === 'board'
          ? 'border-blue-300 bg-blue-50 hover:bg-blue-100'
          : 'border-gray-200 bg-gray-50 hover:bg-gray-100',
      )}
      onClick={() => handleHandZoneClick(owner)}
    >
      <div className="text-xs text-gray-500 mb-1 font-medium pointer-events-none">{label}</div>
      <div className="flex flex-wrap gap-1" onClick={e => e.stopPropagation()}>
        {pieces.length === 0 && <span className="text-xs text-gray-400">なし</span>}
        {pieces.map(({ type, count }: { type: PieceType; count: number }) => (
          <button
            key={type}
            type="button"
            onClick={() => handleHandClick(owner, type)}
            className={cn(
              'flex items-center gap-0.5 px-1.5 py-0.5 rounded text-sm font-serif border transition-colors',
              isSelectedHand(owner, type)
                ? 'bg-yellow-200 border-yellow-500 ring-1 ring-yellow-400'
                : 'bg-white border-gray-300 hover:bg-gray-100',
            )}
          >
            <span className={cn(owner === 'gote' && 'rotate-180 inline-block')}>{type}</span>
            {count > 1 && <span className="text-xs text-gray-600 ml-0.5">{count}</span>}
          </button>
        ))}
      </div>
    </div>
  );

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-xl overflow-auto max-h-[90vh]">
        <DialogHeader>
          <DialogTitle>盤面エディタ</DialogTitle>
        </DialogHeader>

        <p className="text-xs text-gray-500">
          駒をクリックして選択し、移動先をクリックしてください。
          持ち駒エリアをクリックすると盤面の駒を持ち駒に移動できます。
        </p>

        <HandPanel owner="gote" pieces={goteCaptured} label="後手の持ち駒（クリックで盤面の駒を移動）" />

        <div className="flex justify-center">
          <div className="inline-block">
            <div className="flex">
              {COLUMN_LABELS.map(l => (
                <div key={l} className="w-8 h-6 flex items-center justify-center text-xs font-serif font-bold">{l}</div>
              ))}
              <div className="w-5" />
            </div>

            <div className="flex">
              <div className="relative border-2 border-black">
                <div className="grid grid-cols-9">
                  {Array.from({ length: 81 }, (_, idx) => {
                    const ci    = idx % 9;
                    const ri    = Math.floor(idx / 9);
                    const piece = board[ri][ci];
                    const isSel = isSelectedBoard(ri, ci);

                    return (
                      <div
                        key={idx}
                        onClick={() => handleBoardClick(ri, ci)}
                        className={cn(
                          'w-8 h-8 flex items-center justify-center cursor-pointer transition-colors',
                          ci < 8 && 'border-r border-gray-700',
                          ri < 8 && 'border-b border-gray-700',
                          isSel
                            ? 'bg-yellow-200'
                            : selected && !piece
                              ? 'bg-white hover:bg-green-100'
                              : selected && piece
                                ? 'bg-white hover:bg-orange-100'
                                : 'bg-white hover:bg-gray-100',
                        )}
                      >
                        {piece && (
                          <span className={cn(
                            'font-serif text-sm font-bold select-none',
                            piece.owner === 'gote' && 'rotate-180',
                          )}>
                            {piece.type}
                          </span>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>

              <div className="flex flex-col">
                {ROW_LABELS.map(l => (
                  <div key={l} className="w-5 h-8 flex items-center justify-center text-xs font-serif font-bold">{l}</div>
                ))}
              </div>
            </div>
          </div>
        </div>

        <HandPanel owner="sente" pieces={senteCaptured} label="先手の持ち駒（クリックで盤面の駒を移動）" />

        <DialogFooter className="flex-wrap gap-2">
          <Button variant="outline" onClick={clearBoard} type="button" size="sm">空の盤面</Button>
          <Button variant="outline" onClick={resetBoard} type="button" size="sm">初期配置に戻す</Button>
          <Button variant="outline" onClick={handleClose} type="button" size="sm">キャンセル</Button>
          <Button onClick={handleConfirm} type="button" size="sm">挿入</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
