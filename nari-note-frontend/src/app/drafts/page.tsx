'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { mockDrafts } from '@/lib/mockData';
import { formatDistanceToNow } from 'date-fns';
import { ja } from 'date-fns/locale';
import { Edit, Trash2, PlusCircle, Search } from 'lucide-react';

export default function DraftsPage() {
  const [drafts, setDrafts] = useState(mockDrafts);
  const [searchQuery, setSearchQuery] = useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [draftToDelete, setDraftToDelete] = useState<string | null>(null);

  const filteredDrafts = drafts.filter(draft =>
    draft.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    draft.content.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleDeleteClick = (id: string) => {
    setDraftToDelete(id);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = () => {
    if (draftToDelete) {
      setDrafts(drafts.filter(d => d.id !== draftToDelete));
      console.log('下書きを削除:', draftToDelete);
    }
    setDeleteDialogOpen(false);
    setDraftToDelete(null);
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold">下書き一覧</h1>
          <Link href="/articles/new">
            <Button className="flex items-center gap-2">
              <PlusCircle className="w-4 h-4" />
              新規記事作成
            </Button>
          </Link>
        </div>

        {/* 検索バー */}
        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <Input
              placeholder="下書きを検索..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        {/* 下書きリスト */}
        {filteredDrafts.length === 0 ? (
          <Card>
            <CardContent className="pt-6 text-center py-12">
              <p className="text-gray-500 mb-4">
                {searchQuery ? '検索結果がありません' : '下書きがありません'}
              </p>
              {!searchQuery && (
                <Link href="/articles/new">
                  <Button>記事を書く</Button>
                </Link>
              )}
            </CardContent>
          </Card>
        ) : (
          <div className="grid gap-4">
            {filteredDrafts.map((draft) => {
              const formattedDate = formatDistanceToNow(new Date(draft.updatedAt), {
                addSuffix: true,
                locale: ja,
              });

              return (
                <Card key={draft.id} className="hover:shadow-lg transition-shadow">
                  <CardHeader>
                    <div className="flex justify-between items-start gap-4">
                      <div className="flex-1">
                        <Link href={`/articles/${draft.id}/edit`}>
                          <h2 className="text-xl font-bold hover:text-blue-600 transition-colors mb-2">
                            {draft.title}
                          </h2>
                        </Link>
                        <p className="text-sm text-gray-500">
                          最終更新: {formattedDate}
                        </p>
                      </div>
                      <div className="flex gap-2">
                        <Link href={`/articles/${draft.id}/edit`}>
                          <Button variant="outline" size="sm" className="flex items-center gap-1">
                            <Edit className="w-4 h-4" />
                            編集
                          </Button>
                        </Link>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDeleteClick(draft.id)}
                          className="flex items-center gap-1 text-red-600 hover:text-red-700"
                        >
                          <Trash2 className="w-4 h-4" />
                          削除
                        </Button>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="flex flex-wrap gap-2 mb-3">
                      {draft.tags.map((tag) => (
                        <Badge key={tag.id} variant="secondary">
                          #{tag.name}
                        </Badge>
                      ))}
                    </div>
                    <p className="text-gray-600 line-clamp-2">
                      {draft.content}
                    </p>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        )}

        {/* 削除確認ダイアログ */}
        <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>下書きを削除しますか？</DialogTitle>
              <DialogDescription>
                この操作は取り消せません。下書きは完全に削除されます。
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={() => setDeleteDialogOpen(false)}>
                キャンセル
              </Button>
              <Button variant="destructive" onClick={handleDeleteConfirm}>
                削除する
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
