import { useRef, useState } from 'react';
import { MarkdownEditor } from '@/components/molecules/MarkdownEditor';
import type { EmbedBlock, EmbedLang } from '@/components/molecules/MarkdownEditor';
import { parseKifuEmbed } from '@/lib/kif-player';
import { useUploadArticleImage } from '@/lib/api';
import { useRequireVerifiedEmail } from '@/lib/hooks';
import { KifuEmbedDialog } from './KifuEmbedDialog';
import { BoardEditorDialog } from './BoardEditorDialog';
import type { KifuItem } from '../types/kifu';

const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_IMAGE_SIZE_MB = 5;

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
  const [lastEmbedKifuName, setLastEmbedKifuName] = useState<string | undefined>(undefined);
  const [lastEmbedMove, setLastEmbedMove]         = useState<number | undefined>(undefined);
  const [editingBod, setEditingBod]               = useState<string | undefined>(undefined);
  const insertFnRef      = useRef<((name: string, move: number) => void) | null>(null);
  const insertBODFnRef   = useRef<((bod: string) => void) | null>(null);
  const boardInsertFnRef = useRef<((bod: string) => void) | null>(null);
  const uploadImage = useUploadArticleImage();
  const { ensureVerifiedEmail } = useRequireVerifiedEmail();

  const handleImageUpload = async (file: File) => {
    if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
      throw new Error('対応していない画像形式です。JPEG、PNG、WebPのみ対応しています。');
    }
    // バックエンドの上限（5,000,000 バイト）に合わせる
    if (file.size > MAX_IMAGE_SIZE_MB * 1_000_000) {
      throw new Error(`ファイルサイズは${MAX_IMAGE_SIZE_MB}MB以内にしてください。`);
    }
    if (!ensureVerifiedEmail()) {
      throw new Error('画像をアップロードするにはメールアドレスの認証が必要です。');
    }
    const { imageUrl } = await uploadImage.mutateAsync(file);
    return imageUrl;
  };

  const handleKifuEmbed = (insertKifuFn: (name: string, move: number) => void, insertBODFn: (bod: string) => void) => {
    insertFnRef.current    = insertKifuFn;
    insertBODFnRef.current = insertBODFn;
    setShowEmbedDialog(true);
  };

  const handleEmbedConfirm = (kifu: KifuItem, move: number) => {
    insertFnRef.current?.(kifu.name, move);
    setLastEmbedKifuName(kifu.name);
    setLastEmbedMove(move);
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
    setLastEmbedKifuName(name);
    setLastEmbedMove(totalMoves);
    insertFnRef.current    = null;
    insertBODFnRef.current = null;
  };

  const handleBoardEditor = (insertFn: (bod: string) => void) => {
    boardInsertFnRef.current = insertFn;
    setEditingBod(undefined);
    setShowBoardEditorDialog(true);
  };

  const handleEditEmbed = (block: EmbedBlock, replaceFn: (lang: EmbedLang, content: string) => void) => {
    if (block.lang === 'kifu') {
      const { name, moveNumber } = parseKifuEmbed(block.content);
      insertFnRef.current    = (kifuName, move) => replaceFn('kifu', `${kifuName}_${move}`);
      insertBODFnRef.current = (bod) => replaceFn('bod', bod);
      setLastEmbedKifuName(name);
      setLastEmbedMove(moveNumber);
      setShowEmbedDialog(true);
      return;
    }

    boardInsertFnRef.current = (bod) => replaceFn('bod', bod);
    setEditingBod(block.content);
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
        onEditEmbed={handleEditEmbed}
        onImageUpload={handleImageUpload}
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
        initialKifuName={lastEmbedKifuName}
        initialMove={lastEmbedMove}
      />
      <BoardEditorDialog
        open={showBoardEditorDialog}
        onOpenChange={setShowBoardEditorDialog}
        onConfirm={handleBoardEditorConfirm}
        initialBod={editingBod}
      />
    </>
  );
}
