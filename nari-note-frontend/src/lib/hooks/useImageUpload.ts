'use client';

import { useState } from 'react';

interface UseImageUploadOptions {
  maxSizeMB?: number;
}

interface UseImageUploadResult {
  preview: string | undefined;
  validationError: string | undefined;
  handleFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  handleRemove: () => void;
  setPreview: (value: string | undefined) => void;
}

/**
 * useImageUpload - 画像アップロード用カスタムフック
 *
 * ファイルタイプ・サイズのバリデーションとプレビュー生成を担当する
 */
export function useImageUpload(
  onImageSelect: (file: File) => void,
  onImageRemove?: () => void,
  options: UseImageUploadOptions = {},
): UseImageUploadResult {
  const { maxSizeMB = 5 } = options;
  const [preview, setPreview] = useState<string | undefined>(undefined);
  const [validationError, setValidationError] = useState<string | undefined>(undefined);

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

  const handleRemove = () => {
    setPreview(undefined);
    setValidationError(undefined);
    onImageRemove?.();
  };

  return {
    preview,
    validationError,
    handleFileChange,
    handleRemove,
    setPreview,
  };
}
