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
  onKifuAdd?: (kifu: KifuItem) => void;
}

export function ArticleBodyEditor({
  value,
  onChange,
  maxCharacters = 65535,
  kifuList = [],
  onKifuAdd,
}: ArticleBodyEditorProps) {
  const [showEmbedDialog, setShowEmbedDialog]           = useState(false);
  const [showBoardEditorDialog, setShowBoardEditorDialog] = useState(false);
  const insertFnRef      = useRef<((name: string, move: number) => void) | null>(null);
  const insertBODFnRef   = useRef<((bod: string) => void) | null>(null);
  const boardInsertFnRef = useRef<((bod: string) => void) | null>(null);

  const handleKifuEmbed = (insertKifuFn: (name: string, move: number) => void, insertBODFn: (bod: string) => void) => {
    insertFnRef.current    = insertKifuFn;
    insertBODFnRef.current = insertBODFn;
    setShowEmbedDialog(true);
  };

  const handleEmbedConfirm = (kifu: KifuItem, move: number) => {
    insertFnRef.current?.(kifu.name, move);
    insertFnRef.current    = null;
    insertBODFnRef.current = null;
  };

  const handleEmbedConfirmBOD = (bod: string) => {
    insertBODFnRef.current?.(bod);
    insertFnRef.current    = null;
    insertBODFnRef.current = null;
  };

  const handleSaveAsKifu = (name: string, kifText: string, totalMoves: number) => {
    const kifu: KifuItem = { name, text: kifText };
    onKifuAdd?.(kifu);
    insertFnRef.current?.(name, totalMoves);
    insertFnRef.current    = null;
    insertBODFnRef.current = null;
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
        kifuCount={kifuList.length}
        onConfirm={handleEmbedConfirm}
        onConfirmBOD={handleEmbedConfirmBOD}
        onSaveAsKifu={handleSaveAsKifu}
      />
      <BoardEditorDialog
        open={showBoardEditorDialog}
        onOpenChange={setShowBoardEditorDialog}
        onConfirm={handleBoardEditorConfirm}
      />
    </>
  );
}
