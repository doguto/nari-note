import { useRef, useState } from 'react';
import { MarkdownEditor } from '@/components/molecules/MarkdownEditor';
import { KifuEmbedDialog } from './KifuEmbedDialog';
import { BoardEditorDialog } from './BoardEditorDialog';
import type { KifuItem } from '../types/kifu';

interface ArticleBodyEditorProps {
  value: string;
  onChange: (value: string) => void;
  maxCharacters?: number;
  kifuList?: KifuItem[];
}

export function ArticleBodyEditor({
  value,
  onChange,
  maxCharacters = 65535,
  kifuList = [],
}: ArticleBodyEditorProps) {
  const [showEmbedDialog, setShowEmbedDialog]           = useState(false);
  const [showBoardEditorDialog, setShowBoardEditorDialog] = useState(false);
  const insertFnRef      = useRef<((name: string, move: number) => void) | null>(null);
  const boardInsertFnRef = useRef<((bod: string) => void) | null>(null);

  const handleKifuEmbed = (insertFn: (name: string, move: number) => void) => {
    insertFnRef.current = insertFn;
    setShowEmbedDialog(true);
  };

  const handleEmbedConfirm = (kifu: KifuItem, move: number) => {
    insertFnRef.current?.(kifu.name, move);
    insertFnRef.current = null;
  };

  const handleBoardEditor = (insertFn: (bod: string) => void) => {
    boardInsertFnRef.current = insertFn;
    setShowBoardEditorDialog(true);
  };

  const handleBoardEditorConfirm = (bod: string) => {
    boardInsertFnRef.current?.(bod);
    boardInsertFnRef.current = null;
  };

  return (
    <>
      <MarkdownEditor
        value={value}
        onChange={onChange}
        maxCharacters={maxCharacters}
        placeholder="本文を入力してください... 「/」でコマンドメニューを開きます"
        onKifuEmbed={kifuList.length > 0 ? handleKifuEmbed : undefined}
        onBoardEditor={handleBoardEditor}
        kifuList={kifuList.map((k) => ({ name: k.name, kifuText: k.text }))}
      />
      <KifuEmbedDialog
        open={showEmbedDialog}
        onOpenChange={setShowEmbedDialog}
        kifuList={kifuList}
        onConfirm={handleEmbedConfirm}
      />
      <BoardEditorDialog
        open={showBoardEditorDialog}
        onOpenChange={setShowBoardEditorDialog}
        onConfirm={handleBoardEditorConfirm}
      />
    </>
  );
}
