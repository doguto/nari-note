'use client';

import { useState, useRef } from 'react';
import Image from 'next/image';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';

interface ImageUploadFieldProps {
  id: string;
  label: string;
  currentImage?: string;
  onImageSelect: (file: File) => void;
  onImageRemove?: () => void;
  error?: string;
  maxSizeMB?: number;
  required?: boolean;
}

/**
 * ImageUploadField - Atom Component
 * 
 * 画像アップロード機能を持つ基本コンポーネント
 */
export function ImageUploadField({
  id,
  label,
  currentImage,
  onImageSelect,
  onImageRemove,
  error,
  maxSizeMB = 5,
  required = false,
}: ImageUploadFieldProps) {
  const [preview, setPreview] = useState<string | undefined>(currentImage);
  const [validationError, setValidationError] = useState<string>();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setValidationError(undefined);

    // ファイルタイプチェック
    if (!file.type.startsWith('image/')) {
      setValidationError('画像ファイルのみアップロード可能です');
      return;
    }

    // ファイルサイズチェック
    const maxSize = maxSizeMB * 1024 * 1024;
    if (file.size > maxSize) {
      setValidationError(`ファイルサイズは${maxSizeMB}MB以下である必要があります`);
      return;
    }

    // プレビュー表示
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);

    onImageSelect(file);
  };

  const handleRemoveClick = () => {
    setPreview(undefined);
    setValidationError(undefined);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    onImageRemove?.();
  };

  const handleButtonClick = () => {
    fileInputRef.current?.click();
  };

  const displayError = error || validationError;

  return (
    <div className="space-y-2">
      <Label htmlFor={id}>
        {label}
        {required && <span className="text-red-500">*</span>}
      </Label>
      
      <div className="flex items-start gap-4">
        {preview && (
          <div className="relative w-[6rem] h-[6rem]">
            <Image
              src={preview}
              alt="プレビュー"
              width={96}
              height={96}
              className="rounded-full object-cover border-2 border-gray-300"
            />
          </div>
        )}
        
        <div className="flex-1 space-y-2">
          <input
            ref={fileInputRef}
            id={id}
            type="file"
            accept="image/*"
            onChange={handleFileChange}
            className="hidden"
          />
          
          <div className="flex gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={handleButtonClick}
            >
              {preview ? '画像を変更' : '画像を選択'}
            </Button>
            
            {preview && onImageRemove && (
              <Button
                type="button"
                variant="outline"
                onClick={handleRemoveClick}
              >
                削除
              </Button>
            )}
          </div>
          
          <p className="text-xs text-gray-500">
            推奨: 正方形の画像、最大{maxSizeMB}MB
          </p>
        </div>
      </div>
      
      {displayError && (
        <p className="text-sm text-red-500">{displayError}</p>
      )}
    </div>
  );
}
