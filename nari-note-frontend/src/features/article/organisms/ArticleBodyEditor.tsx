import { useRef, useState } from 'react';
import { MarkdownEditor } from '@/components/molecules/MarkdownEditor';
import { KifuEmbedDialog } from './KifuEmbedDialog';
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
  const [showEmbedDialog, setShowEmbedDialog] = useState(false);
  const insertFnRef = useRef<((name: string, move: number) => void) | null>(null);

  const handleKifuEmbed = (insertFn: (name: string, move: number) => void) => {
    insertFnRef.current = insertFn;
    setShowEmbedDialog(true);
  };

  const handleEmbedConfirm = (kifu: KifuItem, move: number) => {
    insertFnRef.current?.(kifu.name, move);
    insertFnRef.current = null;
  };

  return (
    <>
      <MarkdownEditor
        value={value}
        onChange={onChange}
        maxCharacters={maxCharacters}
        placeholder="本文を入力してください... 「/」でコマンドメニューを開きます"
        onKifuEmbed={kifuList.length > 0 ? handleKifuEmbed : undefined}
        kifuList={kifuList.map((k) => ({ name: k.name, kifuText: k.text }))}
      />
      <KifuEmbedDialog
        open={showEmbedDialog}
        onOpenChange={setShowEmbedDialog}
        kifuList={kifuList}
        onConfirm={handleEmbedConfirm}
      />
    </>
  );
}
