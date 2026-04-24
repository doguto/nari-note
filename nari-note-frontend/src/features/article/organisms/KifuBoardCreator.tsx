'use client';

import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { INITIAL_BOARD, COLUMN_LABELS, ROW_LABELS } from '@/lib/kif-player/constants';
import type { CapturedPiece, PieceOwner, PieceType, FreePlayRecorderState } from '@/lib/kif-player';

interface KifuBoardCreatorProps {
  recorder: FreePlayRecorderState;
}

export function KifuBoardCreator({ recorder }: KifuBoardCreatorProps) {
  const {
    board,
    senteCaptured,
    goteCaptured,
    selected,
    moveHistory,
    pendingPromotion,
    handleBoardClick,
    handleHandClick,
    handleHandZoneClick,
    confirmPromotion,
    isSelectedBoard,
    isSelectedHand,
    initializeBoard,
  } = recorder;

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
          <Button
            key={type}
            type="button"
            variant="outline"
            onClick={() => handleHandClick(owner, type)}
            className={cn(
              'h-auto px-1.5 py-0.5 gap-0.5 font-serif',
              isSelectedHand(owner, type)
                ? 'bg-yellow-200 border-yellow-500 ring-1 ring-yellow-400 hover:bg-yellow-200'
                : 'bg-white border-gray-300 hover:bg-gray-100',
            )}
          >
            <span className={cn(owner === 'gote' && 'rotate-180 inline-block')}>{type}</span>
            {count > 1 && <span className="text-xs text-gray-600 ml-0.5">{count}</span>}
          </Button>
        ))}
      </div>
    </div>
  );

  return (
    <div className="space-y-3 mt-4">
      <p className="text-xs text-gray-500">
        駒をクリックして選択し、移動先をクリックしてください。先手（下側）から交互に指してください。
      </p>

      <HandPanel owner="gote" pieces={goteCaptured} label="後手の持ち駒" />

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
                  const piece = board[ri]?.[ci];
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

      <HandPanel owner="sente" pieces={senteCaptured} label="先手の持ち駒" />

      {pendingPromotion && (
        <div className="flex items-center gap-3 p-3 border rounded-lg bg-yellow-50 border-yellow-300">
          <span className="text-sm font-medium">
            {pendingPromotion.prePiece}を成りますか？
          </span>
          <Button size="sm" onClick={() => confirmPromotion(true)}>成る</Button>
          <Button size="sm" variant="outline" onClick={() => confirmPromotion(false)}>成らない</Button>
        </div>
      )}

      <div className="flex items-center justify-between">
        <span className="text-sm text-gray-600">{moveHistory.length}手</span>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => initializeBoard(INITIAL_BOARD, [], [])}
        >
          リセット
        </Button>
      </div>
    </div>
  );
}
