'use client';

import Link from 'next/link';
import { Heart, BookOpen } from 'lucide-react';

interface CourseDetailHeaderProps {
  courseName: string;
  userId: number;
  userName: string;
  likeCount: number;
  articleCount: number;
  createdAt?: string;
}

/**
 * CourseDetailHeader - Organism Component
 * 
 * 講座詳細ページのヘッダー情報を表示するコンポーネント
 * 講座名、作成者、いいね数、記事数などを表示します
 */
export function CourseDetailHeader({
  courseName,
  userId,
  userName,
  likeCount,
  articleCount,
  createdAt,
}: CourseDetailHeaderProps) {
  return (
    <div className="bg-white rounded-lg shadow-lg p-6 mb-6">
      <div className="mb-4">
        <span className="bg-blue-50 text-blue-600 rounded px-3 py-1 text-sm font-medium">
          講座
        </span>
      </div>
      
      <h1 className="text-3xl font-bold text-gray-800 mb-6">
        {courseName}
      </h1>
      
      <div className="flex items-center justify-between flex-wrap gap-4">
        <Link
          href={`/users/${userId}`}
          className="flex items-center gap-3 hover:opacity-80 transition-opacity"
        >
          <div className="w-12 h-12 bg-brand-primary rounded-full flex items-center justify-center text-white font-bold text-lg">
            {userName?.charAt(0).toUpperCase() || 'U'}
          </div>
          <div>
            <div className="font-medium text-gray-800">
              {userName || '不明なユーザー'}
            </div>
            {createdAt && (
              <div className="text-sm text-gray-500">
                {new Date(createdAt).toLocaleDateString('ja-JP', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                })}
              </div>
            )}
          </div>
        </Link>
        
        <div className="flex items-center gap-6 text-gray-600">
          <div className="flex items-center gap-2">
            <BookOpen className="w-5 h-5" />
            <span className="font-medium">{articleCount}</span>
            <span className="text-sm">記事</span>
          </div>
          <div className="flex items-center gap-2">
            <Heart className="w-5 h-5" />
            <span className="font-medium">{likeCount}</span>
            <span className="text-sm">いいね</span>
          </div>
        </div>
      </div>
    </div>
  );
}
