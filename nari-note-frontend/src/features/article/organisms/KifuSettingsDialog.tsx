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
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { KifPlayer } from '@/lib/kif-player';
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

  useEffect(() => {
    if (open) {
      setName(initialKifu?.name ?? '');
      setKifuText(initialKifu?.text ?? '');
      setShowPreview(false);
    }
  }, [open, initialKifu]);

  const handleConfirm = () => {
    onConfirm({ name: name.trim() || defaultName, text: kifuText });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-full max-w-2xl">
        <DialogHeader>
          <DialogTitle>棋譜を設定</DialogTitle>
          <DialogDescription>
            KIF形式の棋譜テキストを入力してください
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
