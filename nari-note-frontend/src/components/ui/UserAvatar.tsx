'use client';

import { useState } from 'react';
import Image from 'next/image';
import { UserRoundIcon } from 'lucide-react';

type AvatarSize = 'sm' | 'md' | 'lg' | 'xl';

interface UserAvatarProps {
  username: string;
  profileImage?: string;
  size?: AvatarSize;
  className?: string;
}

const sizeClasses: Record<AvatarSize, { container: string }> = {
  sm: { container: 'w-8 h-8'},
  md: { container: 'w-12 h-12' },
  lg: { container: 'w-20 h-20' },
  xl: { container: 'w-24 h-24' },
};

/**
 * UserAvatar - Atom Component
 * 
 * ユーザーアバターを表示するコンポーネント
 * プロフィール画像がある場合は画像を表示、ない場合はユーザー名の最初の文字を表示
 */
export function UserAvatar({
  username,
  profileImage,
  size = 'md',
  className = '',
}: UserAvatarProps) {
  const [imageError, setImageError] = useState(false);
  const sizeConfig = sizeClasses[size];
  
  // 画像がある場合かつエラーが発生していない場合は画像を表示
  const shouldShowImage = profileImage && !imageError;
  
  if (shouldShowImage) {
    return (
      <div className={`${sizeConfig.container} relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0 ${className}`}>
        <Image
          src={profileImage}
          alt={`${username}のアイコン`}
          fill
          sizes={size === 'sm' ? '32px' : size === 'md' ? '48px' : size === 'lg' ? '80px' : '96px'}
          className="object-cover"
          onError={() => setImageError(true)}
        />
      </div>
    );
  }
  
  // 画像がない場合はデフォルトアイコンを表示
  return (
    <div 
      className={`${sizeConfig.container} bg-brand-primary rounded-full flex items-center justify-center flex-shrink-0 ${className}`}
      aria-label={`${username}のアバター`}
    >
      <UserRoundIcon />
    </div>
  );
}
