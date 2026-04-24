'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { KifPlayer, useFreePlayRecorder } from '@/lib/kif-player';
import { INITIAL_BOARD, COLUMN_LABELS, ROW_LABELS } from '@/lib/kif-player/constants';
import { cn } from '@/lib/utils';
import type { CapturedPiece, PieceOwner, PieceType } from '@/lib/kif-player';
import type { KifuItem } from '../types/kifu';

interface KifuSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (kifu: KifuItem) => void;
  initialKifu?: KifuItem;
  defaultName?: string;
}

export function KifuSettingsDialog({
  open,
  onOpenChange,
  onConfirm,
  initialKifu,
  defaultName = '棋譜',
}: KifuSettingsDialogProps) {
  const [name, setName] = useState('');
  const [kifuText, setKifuText] = useState('');
  const [showPreview, setShowPreview] = useState(false);
  const [mode, setMode] = useState<'text' | 'editor'>('text');

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
    generateKIF,
  } = useFreePlayRecorder();

  useEffect(() => {
    if (open) {
      setName(initialKifu?.name ?? '');
      setKifuText(initialKifu?.text ?? '');
      setShowPreview(false);
      setMode('text');
      initializeBoard(INITIAL_BOARD, [], []);
    }
  // initializeBoard is stable (useCallback with [] deps)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, initialKifu]);

  const handleConfirm = () => {
    const text = mode === 'editor' ? generateKIF('', 0) : kifuText;
    onConfirm({ name: name.trim() || defaultName, text });
    onOpenChange(false);
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
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>棋譜を設定</DialogTitle>
          <DialogDescription>
            KIF形式のテキストを入力するか、盤面エディターで棋譜を作成してください
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="kifu-name">棋譜の名前</Label>
            <Input
              id="kifu-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={defaultName}
            />
          </div>

          <Tabs value={mode} onValueChange={(v) => setMode(v as 'text' | 'editor')}>
            <TabsList>
              <TabsTrigger value="text">テキスト入力</TabsTrigger>
              <TabsTrigger value="editor">盤面エディター</TabsTrigger>
            </TabsList>

            <TabsContent value="text" className="space-y-4 mt-4">
              <div className="space-y-2">
                <Label htmlFor="kifu-text">棋譜テキスト（KIF形式）</Label>
                <Textarea
                  id="kifu-text"
                  value={kifuText}
                  onChange={(e) => {
                    setKifuText(e.target.value);
                    setShowPreview(false);
                  }}
                  placeholder="棋譜をここに貼り付けてください..."
                  className="min-h-[200px] font-mono text-sm"
                />
              </div>

              {kifuText.trim() && (
                <div className="space-y-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setShowPreview((v) => !v)}
                  >
                    {showPreview ? 'プレビューを閉じる' : 'プレビューを表示'}
                  </Button>

                  {showPreview && (
                    <div className="flex justify-center border rounded-lg p-4 bg-gray-50">
                      <KifPlayer kifText={kifuText} size="sm" />
                    </div>
                  )}
                </div>
              )}
            </TabsContent>

            <TabsContent value="editor" className="space-y-3 mt-4">
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
            </TabsContent>
          </Tabs>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
          >
            キャンセル
          </Button>
          <Button
            type="button"
            onClick={handleConfirm}
            className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
          >
            確定
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
