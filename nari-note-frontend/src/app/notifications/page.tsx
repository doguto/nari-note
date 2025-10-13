'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Pagination } from '@/components/common/Pagination';
import { mockNotifications } from '@/lib/mockData';
import { formatDistanceToNow } from 'date-fns';
import { ja } from 'date-fns/locale';
import { Bell, Check } from 'lucide-react';

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState(mockNotifications);
  const [currentPage, setCurrentPage] = useState(1);
  const notificationsPerPage = 20;

  const totalPages = Math.ceil(notifications.length / notificationsPerPage);
  const indexOfLastNotification = currentPage * notificationsPerPage;
  const indexOfFirstNotification = indexOfLastNotification - notificationsPerPage;
  const currentNotifications = notifications.slice(indexOfFirstNotification, indexOfLastNotification);

  const unreadCount = notifications.filter(n => !n.isRead).length;

  const handleMarkAllAsRead = () => {
    setNotifications(notifications.map(n => ({ ...n, isRead: true })));
    console.log('すべての通知を既読にしました');
  };

  const handleNotificationClick = (id: string) => {
    setNotifications(notifications.map(n =>
      n.id === id ? { ...n, isRead: true } : n
    ));
  };

  const getNotificationLink = (notification: typeof notifications[0]) => {
    switch (notification.type) {
      case 'article':
        return `/articles/${notification.articleId}`;
      case 'like':
        return `/articles/${notification.articleId}`;
      case 'follow':
        return `/users/${notification.user.username}`;
      default:
        return '#';
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold flex items-center gap-2">
            <Bell className="w-8 h-8" />
            通知
          </h1>
          {unreadCount > 0 && (
            <Button
              variant="outline"
              onClick={handleMarkAllAsRead}
              className="flex items-center gap-2"
            >
              <Check className="w-4 h-4" />
              すべて既読にする
            </Button>
          )}
        </div>

        {notifications.length === 0 ? (
          <Card>
            <CardContent className="pt-6 text-center py-12">
              <Bell className="w-16 h-16 mx-auto mb-4 text-gray-300" />
              <p className="text-gray-500">通知はありません</p>
            </CardContent>
          </Card>
        ) : (
          <>
            <div className="space-y-2 mb-8">
              {currentNotifications.map((notification) => {
                const formattedDate = formatDistanceToNow(new Date(notification.createdAt), {
                  addSuffix: true,
                  locale: ja,
                });

                return (
                  <Link
                    key={notification.id}
                    href={getNotificationLink(notification)}
                    onClick={() => handleNotificationClick(notification.id)}
                  >
                    <Card className={`hover:shadow-md transition-shadow ${
                      !notification.isRead ? 'bg-blue-50 border-blue-200' : ''
                    }`}>
                      <CardContent className="pt-6">
                        <div className="flex gap-4">
                          <Avatar className="h-12 w-12">
                            <AvatarImage
                              src={notification.user.avatarUrl}
                              alt={notification.user.displayName}
                            />
                            <AvatarFallback>
                              {notification.user.displayName[0]}
                            </AvatarFallback>
                          </Avatar>

                          <div className="flex-1">
                            <p className={`mb-1 ${!notification.isRead ? 'font-semibold' : ''}`}>
                              {notification.message}
                            </p>
                            <p className="text-sm text-gray-500">{formattedDate}</p>
                          </div>

                          {!notification.isRead && (
                            <div className="flex items-center">
                              <div className="w-2 h-2 bg-blue-600 rounded-full"></div>
                            </div>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  </Link>
                );
              })}
            </div>

            {totalPages > 1 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={setCurrentPage}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
}
