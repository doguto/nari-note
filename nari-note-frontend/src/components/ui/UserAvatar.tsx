'use client';

import { useState } from 'react';
import Image from 'next/image';

type AvatarSize = 'sm' | 'md' | 'lg' | 'xl';

interface UserAvatarProps {
  username: string;
  userIconImageUrl?: string;
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
 * プロフィール画像がある場合は画像を表示、ない場合はデフォルトのシルエットを表示
 */
export function UserAvatar({
  username,
  userIconImageUrl,
  size = 'md',
  className = '',
}: UserAvatarProps) {
  const [imageError, setImageError] = useState(false);
  const sizeConfig = sizeClasses[size];

  // 画像がある場合かつエラーが発生していない場合は画像を表示
  const shouldShowImage = userIconImageUrl && !imageError;

  if (shouldShowImage) {
    return (
      <div className={`${sizeConfig.container} relative rounded-full overflow-hidden bg-gray-200 flex-shrink-0 ${className}`}>
        <Image
          src={userIconImageUrl}
          alt={`${username}のアイコン`}
          fill
          unoptimized
          className="object-cover"
          onError={() => setImageError(true)}
        />
      </div>
    );
  }

  return (
    <div
      role="img"
      className={`${sizeConfig.container} bg-brand-primary-light rounded-full overflow-hidden flex items-end justify-center flex-shrink-0 ${className}`}
      aria-label={`${username}のアバター`}
    >
      {/* Button 内に置かれると [&_svg]:size-4 で縮められるため、インラインで大きさを固定する */}
      <svg viewBox="0 0 24 24" style={{ width: '84%', height: '84%' }} className="shrink-0 fill-brand-primary-muted" aria-hidden="true">
        <circle cx="12" cy="9" r="4.5" />
        <path d="M3 24c0-5 4-8.5 9-8.5s9 3.5 9 8.5Z" />
      </svg>
    </div>
  );
}
