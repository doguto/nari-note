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
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';

interface PublishSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onPublish: (publishedAt?: string) => void;
  isLoading?: boolean;
}

type PublishType = 'immediate' | 'scheduled';

/**
 * PublishSettingsDialog - Organism Component
 * 
 * 記事の投稿設定を行うダイアログコンポーネント
 * 即座に公開するか、予約投稿するかを選択可能
 * 
 * @param open - ダイアログの開閉状態
 * @param onOpenChange - ダイアログの開閉状態を変更するコールバック
 * @param onPublish - 公開ボタンが押されたときのコールバック（ISO 8601形式の日時文字列を受け取る）
 * @param isLoading - ローディング状態
 */
export function PublishSettingsDialog({
  open,
  onOpenChange,
  onPublish,
  isLoading = false,
}: PublishSettingsDialogProps) {
  const [publishType, setPublishType] = useState<PublishType>('immediate');
  const [scheduledDateTime, setScheduledDateTime] = useState('');
  const [validationError, setValidationError] = useState('');

  // ダイアログが開かれたときに現在時刻の1時間後をデフォルト値として設定
  useEffect(() => {
    if (open) {
      const now = new Date();
      now.setHours(now.getHours() + 1);
      now.setMinutes(0);
      now.setSeconds(0);
      now.setMilliseconds(0);
      
      // datetime-local input用のフォーマット (YYYY-MM-DDTHH:MM)
      const year = now.getFullYear();
      const month = String(now.getMonth() + 1).padStart(2, '0');
      const day = String(now.getDate()).padStart(2, '0');
      const hours = String(now.getHours()).padStart(2, '0');
      const minutes = String(now.getMinutes()).padStart(2, '0');
      
      setScheduledDateTime(`${year}-${month}-${day}T${hours}:${minutes}`);
      setValidationError('');
      setPublishType('immediate');
    }
  }, [open]);

  const validateScheduledDateTime = (): boolean => {
    if (publishType === 'immediate') {
      return true;
    }

    if (!scheduledDateTime) {
      setValidationError('日時を選択してください');
      return false;
    }

    // datetime-local input はローカル時刻を返し、new Date() で作成される Date オブジェクトも
    // ローカル時刻として解釈されるため、比較はローカル時刻同士で行われる
    const selectedDate = new Date(scheduledDateTime);
    const now = new Date();

    if (selectedDate <= now) {
      setValidationError('未来の日時を選択してください');
      return false;
    }

    setValidationError('');
    return true;
  };

  const handlePublish = () => {
    if (!validateScheduledDateTime()) {
      return;
    }

    if (publishType === 'immediate') {
      // 即座に公開: publishedAtを指定しない（バックエンドが現在時刻を設定）
      onPublish(undefined);
    } else {
      // 予約投稿: 選択した日時をISO 8601形式（UTC）で送信
      // datetime-local input から取得したローカル時刻を UTC に変換
      const selectedDate = new Date(scheduledDateTime);
      onPublish(selectedDate.toISOString());
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>投稿設定</DialogTitle>
          <DialogDescription>
            記事の公開方法を選択してください
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          <RadioGroup
            value={publishType}
            onValueChange={(value) => {
              setPublishType(value as PublishType);
              setValidationError('');
            }}
          >
            <div className="flex items-center space-x-3 space-y-0">
              <RadioGroupItem value="immediate" id="immediate" />
              <Label
                htmlFor="immediate"
                className="text-base font-normal cursor-pointer"
              >
                即座に公開
              </Label>
            </div>
            
            <div className="flex items-start space-x-3 space-y-0">
              <RadioGroupItem value="scheduled" id="scheduled" className="mt-1" />
              <div className="flex-1 space-y-2">
                <Label
                  htmlFor="scheduled"
                  className="text-base font-normal cursor-pointer"
                >
                  予約投稿
                </Label>
                
                {publishType === 'scheduled' && (
                  <div className="space-y-2 pt-2">
                    <Label htmlFor="datetime" className="text-sm text-gray-600">
                      公開日時（現地時刻）
                    </Label>
                    <Input
                      id="datetime"
                      type="datetime-local"
                      value={scheduledDateTime}
                      onChange={(e) => {
                        setScheduledDateTime(e.target.value);
                        setValidationError('');
                      }}
                      className="w-full"
                      min={new Date().toISOString().slice(0, 16)}
                    />
                    <p className="text-xs text-gray-500">
                      選択した日時に記事が公開されます
                    </p>
                    {validationError && (
                      <p className="text-sm text-red-500">{validationError}</p>
                    )}
                  </div>
                )}
              </div>
            </div>
          </RadioGroup>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isLoading}
          >
            キャンセル
          </Button>
          <Button
            type="button"
            onClick={handlePublish}
            disabled={isLoading}
            className="bg-[var(--brand-primary)] hover:bg-[var(--brand-primary-hover)]"
          >
            {isLoading ? '公開中...' : '公開する'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
