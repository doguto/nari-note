import { MarkdownEditor } from '@/components/molecules/MarkdownEditor';

interface ArticleBodyEditorProps {
  value: string;
  onChange: (value: string) => void;
  maxCharacters?: number;
}

export function ArticleBodyEditor({
  value,
  onChange,
  maxCharacters = 65535,
}: ArticleBodyEditorProps) {
  return (
    <MarkdownEditor
      value={value}
      onChange={onChange}
      maxCharacters={maxCharacters}
      placeholder="本文を入力してください... 「/」でコマンドメニューを開きます"
    />
  );
}
