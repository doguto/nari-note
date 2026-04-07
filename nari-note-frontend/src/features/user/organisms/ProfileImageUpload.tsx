'use client';

import { ImageUploadField } from '@/components/ui';

interface ProfileImageUploadProps {
  currentImage?: string;
  onImageSelect: (file: File) => void;
  onImageRemove?: () => void;
  error?: string;
}

/**
 * ProfileImageUpload - Molecule Component
 * 
 * プロフィール画像アップロードコンポーネント
 */
export function ProfileImageUpload({
  currentImage,
  onImageSelect,
  onImageRemove,
  error,
}: ProfileImageUploadProps) {
  return (
    <ImageUploadField
      id="profileImage"
      label="プロフィール画像"
      currentImage={currentImage}
      onImageSelect={onImageSelect}
      onImageRemove={onImageRemove}
      error={error}
      maxSizeMB={5}
    />
  );
}
