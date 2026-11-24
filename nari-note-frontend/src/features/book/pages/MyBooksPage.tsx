'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { ErrorMessage, LoadingSpinner } from '@/components/ui';
import { BoardEditorDialog } from '@/components/organisms';
import { parseBOD } from '@/lib/kif-player';
import type { PieceOwner } from '@/lib/kif-player';
import { useCreateBook, useDeleteBook, useGetMyBooks } from '@/lib/api';
import type { BookSummaryDto } from '@/lib/api/types';
import { PageWithoutSidebar } from '@/features/global/organisms';
import { MyBooksTemplate } from '../templates';
import { boardToSfen } from '../utils/sfen';

export function MyBooksPage() {
  const router = useRouter();
  const [name, setName] = useState('');
  const [rootBod, setRootBod] = useState<string | null>(null);
  const [rootTurn, setRootTurn] = useState<PieceOwner>('sente');
  const [isEditorOpen, setIsEditorOpen] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string>();
  const [pendingDelete, setPendingDelete] = useState<BookSummaryDto | null>(null);

  const { data, isLoading, error, refetch } = useGetMyBooks();
  const createBook = useCreateBook({
    onSuccess: (res) => router.push(`/books/${res.id}?p=${res.rootPositionId}`),
    onError: () => setErrorMessage('定跡を作成できませんでした。開始局面の玉の配置や手番を確認してください。'),
  });
  const deleteBook = useDeleteBook({
    onSuccess: () => setPendingDelete(null),
    onError: () => setErrorMessage('定跡を削除できませんでした。'),
  });

  if (isLoading) return <LoadingSpinner text="定跡を読み込み中..." />;
  if (error) return <ErrorMessage message="定跡の取得に失敗しました" onRetry={refetch} />;

  return (
    <PageWithoutSidebar>
      <MyBooksTemplate
        books={data?.books ?? []}
        name={name}
        rootBod={rootBod}
        rootTurn={rootTurn}
        isCreating={createBook.isPending}
        errorMessage={errorMessage}
        onNameChange={setName}
        onEditRoot={() => setIsEditorOpen(true)}
        onResetRoot={() => setRootBod(null)}
        onRootTurnChange={setRootTurn}
        onCreate={() => {
          setErrorMessage(undefined);
          const parsed = rootBod ? parseBOD(rootBod) : null;
          createBook.mutate({
            name: name.trim(),
            rootSfen: parsed ? boardToSfen(parsed.board, parsed.captured, rootTurn) : undefined,
          });
        }}
        onDelete={setPendingDelete}
      />
      <BoardEditorDialog
        open={isEditorOpen}
        onOpenChange={setIsEditorOpen}
        initialBod={rootBod ?? undefined}
        confirmLabel="この局面から始める"
        onConfirm={setRootBod}
      />
      <Dialog open={pendingDelete !== null} onOpenChange={(open) => !open && setPendingDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>定跡を削除しますか？</DialogTitle>
            <DialogDescription>
              「{pendingDelete?.name}」の局面・手・研究メモをすべて削除します。この操作は取り消せません。
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPendingDelete(null)}>
              キャンセル
            </Button>
            <Button
              disabled={deleteBook.isPending}
              className="bg-red-500 text-white hover:bg-red-600"
              onClick={() => pendingDelete && deleteBook.mutate({ id: pendingDelete.id })}
            >
              削除する
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </PageWithoutSidebar>
  );
}
