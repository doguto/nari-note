'use client';

import { useState, useEffect, use } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Save, Eye, Send, Trash2 } from 'lucide-react';
import { mockArticles, mockDrafts } from '@/lib/mockData';
import { MarkdownContent } from '@/components/common/MarkdownContent';

const MAX_CONTENT_LENGTH = 50000;

export default function EditArticlePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  
  // 記事または下書きを取得
  const article = [...mockArticles, ...mockDrafts].find(a => a.id === id);
  
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [tagInput, setTagInput] = useState('');
  const [showPreview, setShowPreview] = useState(false);
  const [showLeaveDialog, setShowLeaveDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [errors, setErrors] = useState<{ title?: string; content?: string; tags?: string }>({});

  const contentLength = content.length;
  const isOverLimit = contentLength > MAX_CONTENT_LENGTH;

  // 記事データの初期化
  useEffect(() => {
    if (article) {
      setTitle(article.title);
      setContent(article.content);
      setTags(article.tags.map(t => t.name));
    }
  }, [article]);

  // 自動保存
  useEffect(() => {
    const interval = setInterval(() => {
      if (hasUnsavedChanges) {
        console.log('自動保存:', { title, content, tags });
        setHasUnsavedChanges(false);
      }
    }, 30000);

    return () => clearInterval(interval);
  }, [hasUnsavedChanges, title, content, tags]);

  useEffect(() => {
    setHasUnsavedChanges(true);
  }, [title, content, tags]);

  if (!article) {
    return (
      <div className="container mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold mb-4">記事が見つかりません</h1>
        <Button onClick={() => router.push('/')}>トップページに戻る</Button>
      </div>
    );
  }

  const handleAddTag = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && tagInput.trim()) {
      e.preventDefault();
      if (!tags.includes(tagInput.trim())) {
        setTags([...tags, tagInput.trim()]);
      }
      setTagInput('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setTags(tags.filter(tag => tag !== tagToRemove));
  };

  const validateForm = () => {
    const newErrors: typeof errors = {};

    if (!title.trim()) {
      newErrors.title = 'タイトルは必須です';
    }
    if (tags.length === 0) {
      newErrors.tags = 'タグは少なくとも1つ必要です';
    }
    if (isOverLimit) {
      newErrors.content = `文字数が上限（${MAX_CONTENT_LENGTH}文字）を超えています`;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSaveDraft = () => {
    if (!validateForm()) return;
    
    console.log('下書き保存:', { id, title, content, tags });
    router.push('/drafts');
  };

  const handlePublish = () => {
    if (!validateForm()) return;
    
    console.log('記事を公開:', { id, title, content, tags });
    router.push(`/articles/${id}`);
  };

  const handleDelete = () => {
    console.log('記事を削除:', id);
    setShowDeleteDialog(false);
    router.push('/drafts');
  };

  const handleInsertBoard = () => {
    const boardFormat = `
/board
後手の持駒：なし
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
|v香v桂v銀v金v玉v金v銀v桂v香|一
| ・v飛 ・ ・ ・ ・ ・v角 ・|二
|v歩v歩v歩v歩v歩v歩v歩v歩v歩|三
| ・ ・ ・ ・ ・ ・ ・ ・ ・|四
| ・ ・ ・ ・ ・ ・ ・ ・ ・|五
| ・ ・ 歩 ・ ・ ・ ・ ・ ・|六
| 歩 歩 ・ 歩 歩 歩 歩 歩 歩|七
| ・ 角 ・ ・ ・ ・ ・ 飛 ・|八
| 香 桂 銀 金 玉 金 銀 桂 香|九
+---------------------------+
先手の持駒：なし
`;
    setContent(content + boardFormat);
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold" style={{ color: '#c41e3a' }}>記事を編集</h1>
          {article.isDraft && (
            <Badge variant="outline" className="text-gray-600">下書き</Badge>
          )}
        </div>

        {/* タイトル */}
        <div className="mb-4">
          <Input
            placeholder="記事のタイトルを入力"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className={`text-2xl font-bold ${errors.title ? 'border-red-500' : ''}`}
          />
          {errors.title && (
            <p className="text-sm text-red-600 mt-1">{errors.title}</p>
          )}
        </div>

        {/* タグ */}
        <div className="mb-4">
          <Input
            placeholder="タグを入力してEnterキー（例: 定跡, 囲い）"
            value={tagInput}
            onChange={(e) => setTagInput(e.target.value)}
            onKeyDown={handleAddTag}
            className={errors.tags ? 'border-red-500' : ''}
          />
          {errors.tags && (
            <p className="text-sm text-red-600 mt-1">{errors.tags}</p>
          )}
          <div className="flex flex-wrap gap-2 mt-2">
            {tags.map((tag) => (
              <Badge
                key={tag}
                variant="secondary"
                className="cursor-pointer"
                onClick={() => handleRemoveTag(tag)}
              >
                #{tag} ×
              </Badge>
            ))}
          </div>
        </div>

        {/* 本文 */}
        <div className="mb-4">
          <div className="flex justify-between items-center mb-2">
            <label className="text-sm font-medium">本文</label>
            <div className="flex items-center gap-4">
              <span className={`text-sm ${isOverLimit ? 'text-red-600 font-bold' : 'text-gray-600'}`}>
                {contentLength.toLocaleString()} / {MAX_CONTENT_LENGTH.toLocaleString()} 文字
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={handleInsertBoard}
              >
                盤面を挿入
              </Button>
            </div>
          </div>
          <Textarea
            placeholder="記事の内容を入力してください..."
            value={content}
            onChange={(e) => setContent(e.target.value)}
            className={`min-h-[400px] ${errors.content ? 'border-red-500' : ''}`}
          />
          {errors.content && (
            <p className="text-sm text-red-600 mt-1">{errors.content}</p>
          )}
        </div>

        {/* アクションボタン */}
        <div className="flex gap-4 justify-between">
          <Button
            variant="outline"
            onClick={() => setShowDeleteDialog(true)}
            className="flex items-center gap-2 text-red-600 hover:text-red-700"
          >
            <Trash2 className="w-4 h-4" />
            削除
          </Button>
          
          <div className="flex gap-4">
            <Button
              variant="outline"
              onClick={() => setShowPreview(true)}
              className="flex items-center gap-2"
            >
              <Eye className="w-4 h-4" />
              プレビュー
            </Button>
            <Button
              variant="outline"
              onClick={handleSaveDraft}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              下書き保存
            </Button>
            <Button
              onClick={handlePublish}
              className="flex items-center gap-2"
              style={{ backgroundColor: '#c41e3a' }}
            >
              <Send className="w-4 h-4" />
              {article.isDraft ? '公開する' : '更新する'}
            </Button>
          </div>
        </div>

        {/* プレビューダイアログ */}
        <Dialog open={showPreview} onOpenChange={setShowPreview}>
          <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>プレビュー</DialogTitle>
            </DialogHeader>
            <div>
              <h1 className="text-3xl font-bold mb-4">{title || '（タイトルなし）'}</h1>
              <div className="flex flex-wrap gap-2 mb-4">
                {tags.map((tag) => (
                  <Badge key={tag} variant="secondary">#{tag}</Badge>
                ))}
              </div>
              <Card className="wa-pattern">
                <CardContent className="pt-6">
                  <MarkdownContent content={content || '*本文がありません*'} />
                </CardContent>
              </Card>
            </div>
            <DialogFooter>
              <Button onClick={() => setShowPreview(false)}>閉じる</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* 削除確認ダイアログ */}
        <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>記事を削除しますか？</DialogTitle>
              <DialogDescription>
                この操作は取り消せません。記事は完全に削除されます。
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={() => setShowDeleteDialog(false)}>
                キャンセル
              </Button>
              <Button variant="destructive" onClick={handleDelete}>
                削除する
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* ページ離脱確認ダイアログ */}
        <Dialog open={showLeaveDialog} onOpenChange={setShowLeaveDialog}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>変更を破棄しますか？</DialogTitle>
              <DialogDescription>
                保存されていない変更は失われます。
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={() => setShowLeaveDialog(false)}>
                キャンセル
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
