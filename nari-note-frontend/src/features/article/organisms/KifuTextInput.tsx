'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { KifPlayer } from '@/lib/kif-player';

interface KifuTextInputProps {
  kifuText: string;
  onChange: (text: string) => void;
}

export function KifuTextInput({ kifuText, onChange }: KifuTextInputProps) {
  const [showPreview, setShowPreview] = useState(false);

  return (
    <div className="space-y-4 mt-4">
      <div className="space-y-2">
        <Label htmlFor="kifu-text">棋譜テキスト（KIF形式）</Label>
        <Textarea
          id="kifu-text"
          value={kifuText}
          onChange={(e) => {
            onChange(e.target.value);
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
  );
}
