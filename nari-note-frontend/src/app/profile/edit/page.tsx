'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { currentUser } from '@/lib/mockData';
import { Camera, Trash2 } from 'lucide-react';

export default function ProfileEditPage() {
  const router = useRouter();
  const [username, setUsername] = useState(currentUser.username);
  const [displayName, setDisplayName] = useState(currentUser.displayName);
  const [bio, setBio] = useState(currentUser.bio || '');
  const [avatarUrl, setAvatarUrl] = useState(currentUser.avatarUrl);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [errors, setErrors] = useState<{
    username?: string;
    displayName?: string;
    bio?: string;
  }>({});

  const validateForm = () => {
    const newErrors: typeof errors = {};

    if (!username.trim()) {
      newErrors.username = 'ユーザー名が必須です';
    } else if (username.length < 3) {
      newErrors.username = 'ユーザー名は3文字以上である必要があります';
    } else if (!/^[a-zA-Z0-9_]+$/.test(username)) {
      newErrors.username = 'ユーザー名は英数字とアンダースコアのみ使用可能です';
    }

    if (displayName && displayName.length > 20) {
      newErrors.displayName = '表示名は20文字以内である必要があります';
    }

    if (bio && bio.length > 250) {
      newErrors.bio = '自己紹介文は250文字以内である必要があります';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = () => {
    if (!validateForm()) return;

    console.log('プロフィールを更新:', { username, displayName, bio, avatarUrl });
    router.push(`/users/${username}`);
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // モック：画像アップロード
      if (file.size > 5 * 1024 * 1024) {
        alert('ファイルサイズは5MB以下である必要があります');
        return;
      }
      if (!file.type.startsWith('image/')) {
        alert('画像ファイルのみアップロード可能です');
        return;
      }
      console.log('画像をアップロード:', file.name);
      // プレビュー表示（実際はURLを生成）
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarUrl(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleDeleteImage = () => {
    setAvatarUrl(undefined);
    setShowDeleteDialog(false);
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-2xl mx-auto">
        <Card>
          <CardHeader>
            <CardTitle className="text-2xl">プロフィール編集</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* アイコン画像 */}
            <div>
              <label className="block text-sm font-medium mb-2">プロフィール画像</label>
              <div className="flex items-center gap-4">
                <Avatar className="h-24 w-24">
                  <AvatarImage src={avatarUrl} alt={displayName} />
                  <AvatarFallback className="text-3xl">{displayName[0]}</AvatarFallback>
                </Avatar>
                <div className="flex gap-2">
                  <label htmlFor="avatar-upload">
                    <Button type="button" variant="outline" className="cursor-pointer" asChild>
                      <span>
                        <Camera className="w-4 h-4 mr-2" />
                        画像を選択
                      </span>
                    </Button>
                  </label>
                  <input
                    id="avatar-upload"
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={handleImageUpload}
                  />
                  {avatarUrl && (
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => setShowDeleteDialog(true)}
                    >
                      <Trash2 className="w-4 h-4 mr-2" />
                      削除
                    </Button>
                  )}
                </div>
              </div>
              <p className="text-sm text-gray-500 mt-2">
                PNG、JPG、GIF形式に対応。最大5MB。
              </p>
            </div>

            {/* ユーザー名 */}
            <div>
              <label htmlFor="username" className="block text-sm font-medium mb-2">
                ユーザー名 *
              </label>
              <Input
                id="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className={errors.username ? 'border-red-500' : ''}
              />
              {errors.username && (
                <p className="text-sm text-red-600 mt-1">{errors.username}</p>
              )}
              <p className="text-sm text-gray-500 mt-1">
                3文字以上の英数字とアンダースコアのみ使用可能
              </p>
            </div>

            {/* 表示名 */}
            <div>
              <label htmlFor="displayName" className="block text-sm font-medium mb-2">
                表示名
              </label>
              <Input
                id="displayName"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                className={errors.displayName ? 'border-red-500' : ''}
              />
              {errors.displayName && (
                <p className="text-sm text-red-600 mt-1">{errors.displayName}</p>
              )}
              <p className="text-sm text-gray-500 mt-1">20文字以内</p>
            </div>

            {/* 自己紹介文 */}
            <div>
              <label htmlFor="bio" className="block text-sm font-medium mb-2">
                自己紹介文
              </label>
              <Textarea
                id="bio"
                value={bio}
                onChange={(e) => setBio(e.target.value)}
                className={`min-h-[120px] ${errors.bio ? 'border-red-500' : ''}`}
                placeholder="あなたについて教えてください..."
              />
              {errors.bio && (
                <p className="text-sm text-red-600 mt-1">{errors.bio}</p>
              )}
              <p className="text-sm text-gray-500 mt-1">
                {bio.length} / 250 文字
              </p>
            </div>

            {/* アクションボタン */}
            <div className="flex gap-4 justify-end pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowCancelDialog(true)}
              >
                キャンセル
              </Button>
              <Button onClick={handleSave}>
                保存する
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* 画像削除確認ダイアログ */}
        <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>プロフィール画像を削除しますか？</DialogTitle>
              <DialogDescription>
                デフォルトのアイコンが表示されます。
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={() => setShowDeleteDialog(false)}>
                キャンセル
              </Button>
              <Button variant="destructive" onClick={handleDeleteImage}>
                削除する
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* キャンセル確認ダイアログ */}
        <Dialog open={showCancelDialog} onOpenChange={setShowCancelDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>変更を破棄しますか？</DialogTitle>
              <DialogDescription>
                保存されていない変更は失われます。
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={() => setShowCancelDialog(false)}>
                編集を続ける
              </Button>
              <Button variant="destructive" onClick={() => router.back()}>
                破棄する
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
