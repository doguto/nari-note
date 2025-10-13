'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { currentUser, mockNotifications } from '@/lib/mockData';

export const Header: React.FC = () => {
  const [isLoggedIn] = useState(true); // モック：ログイン状態
  const unreadCount = mockNotifications.filter(n => !n.isRead).length;

  return (
    <header className="sticky top-0 z-50 w-full border-b-2 border-red-900 bg-white shadow-sm">
      <div className="container mx-auto px-4">
        <div className="flex h-16 items-center justify-between">
          {/* ロゴ */}
          <Link href="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <div className="text-2xl font-bold" style={{ color: '#c41e3a' }}>
              <span className="text-3xl">成</span>りノート
            </div>
          </Link>

          {/* 検索バー */}
          <div className="hidden md:flex flex-1 max-w-md mx-8">
            <Input
              type="search"
              placeholder="記事を検索..."
              className="w-full"
            />
          </div>

          {/* ナビゲーション */}
          <nav className="flex items-center gap-4">
            {isLoggedIn ? (
              <>
                <Link href="/articles/new">
                  <Button style={{ backgroundColor: '#c41e3a' }} className="hover:opacity-90">記事を書く</Button>
                </Link>

                {/* 通知アイコン */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon" className="relative">
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                      </svg>
                      {unreadCount > 0 && (
                        <Badge className="absolute -top-1 -right-1 h-5 w-5 flex items-center justify-center p-0 text-xs">
                          {unreadCount}
                        </Badge>
                      )}
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-80">
                    <div className="px-4 py-2 font-semibold">通知</div>
                    <DropdownMenuSeparator />
                    {mockNotifications.slice(0, 5).map((notification) => (
                      <DropdownMenuItem key={notification.id} className="cursor-pointer">
                        <div className="flex gap-3 py-2">
                          <Avatar className="h-10 w-10">
                            <AvatarImage src={notification.user.avatarUrl} />
                            <AvatarFallback>{notification.user.displayName[0]}</AvatarFallback>
                          </Avatar>
                          <div className="flex-1">
                            <p className={`text-sm ${!notification.isRead ? 'font-semibold' : ''}`}>
                              {notification.message}
                            </p>
                            <p className="text-xs text-gray-500 mt-1">
                              {new Date(notification.createdAt).toLocaleDateString('ja-JP')}
                            </p>
                          </div>
                        </div>
                      </DropdownMenuItem>
                    ))}
                    <DropdownMenuSeparator />
                    <DropdownMenuItem asChild>
                      <Link href="/notifications" className="w-full text-center cursor-pointer">
                        すべての通知を見る
                      </Link>
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>

                {/* ユーザーメニュー */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                      <Avatar>
                        <AvatarImage src={currentUser.avatarUrl} alt={currentUser.displayName} />
                        <AvatarFallback>{currentUser.displayName[0]}</AvatarFallback>
                      </Avatar>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <div className="px-2 py-1.5">
                      <p className="text-sm font-semibold">{currentUser.displayName}</p>
                      <p className="text-xs text-gray-500">@{currentUser.username}</p>
                    </div>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem asChild>
                      <Link href="/my-page">マイページ</Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link href="/drafts">下書き一覧</Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link href="/profile/edit">設定</Link>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem>ログアウト</DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </>
            ) : (
              <>
                <Link href="/signin">
                  <Button variant="ghost">ログイン</Button>
                </Link>
                <Link href="/signup">
                  <Button>新規登録</Button>
                </Link>
              </>
            )}
          </nav>
        </div>
      </div>
    </header>
  );
};
