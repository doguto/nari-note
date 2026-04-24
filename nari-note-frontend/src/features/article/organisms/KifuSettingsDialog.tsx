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
import { useFreePlayRecorder } from '@/lib/kif-player';
import { INITIAL_BOARD } from '@/lib/kif-player/constants';
import { KifuTextInput } from './KifuTextInput';
import { KifuBoardCreator } from './KifuBoardCreator';
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
  const [mode, setMode] = useState<'text' | 'editor'>('text');

  const recorder = useFreePlayRecorder();

  useEffect(() => {
    if (open) {
      setName(initialKifu?.name ?? '');
      setKifuText(initialKifu?.text ?? '');
      setMode('text');
      recorder.initializeBoard(INITIAL_BOARD, [], []);
    }
  // initializeBoard is stable (useCallback with [] deps)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, initialKifu]);

  const handleConfirm = () => {
    const text = mode === 'editor' ? recorder.generateKIF('', 0) : kifuText;
    onConfirm({ name: name.trim() || defaultName, text });
    onOpenChange(false);
  };

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
              <TabsTrigger value="editor">新規作成</TabsTrigger>
            </TabsList>

            <TabsContent value="text">
              <KifuTextInput kifuText={kifuText} onChange={setKifuText} />
            </TabsContent>

            <TabsContent value="editor">
              <KifuBoardCreator recorder={recorder} />
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
