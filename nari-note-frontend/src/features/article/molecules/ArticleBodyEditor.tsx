'use client';

import { useEditor, EditorContent, Editor } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Placeholder from '@tiptap/extension-placeholder';
import CharacterCount from '@tiptap/extension-character-count';
import { ReactRenderer } from '@tiptap/react';
import { SuggestionProps } from '@tiptap/suggestion';
import tippy, { Instance as TippyInstance } from 'tippy.js';
import 'tippy.js/dist/tippy.css';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { CharacterCounter } from '@/components/common/molecules';
import { commands, createSlashCommandExtension, Command } from '../extensions/SlashCommand';
import { CommandsList } from '../extensions/CommandsList';
import { useEffect } from 'react';

interface ArticleBodyEditorProps {
  value: string;
  onChange: (value: string) => void;
  showPreview: boolean;
  onTogglePreview: () => void;
  maxCharacters?: number;
}

/**
 * ArticleBodyEditor - Molecule Component
 * 
 * 記事本文エディター（TipTap + Markdown編集とプレビュー機能付き）
 */
export function ArticleBodyEditor({
  value,
  onChange,
  showPreview,
  onTogglePreview,
  maxCharacters = 65535,
}: ArticleBodyEditorProps) {
  const editor = useEditor({
    immediatelyRender: false,
    extensions: [
      StarterKit.configure({
        heading: {
          levels: [1, 2, 3],
        },
      }),
      Placeholder.configure({
        placeholder: '本文を入力してください... 「/」でコマンドメニューを開きます',
      }),
      CharacterCount.configure({
        limit: maxCharacters,
      }),
      createSlashCommandExtension({
        char: '/',
        startOfLine: false,
        items: ({ query }: { query: string }) => {
          return commands.filter((item: Command) =>
            item.title.toLowerCase().includes(query.toLowerCase()) ||
            item.id.toLowerCase().includes(query.toLowerCase()) ||
            item.description.toLowerCase().includes(query.toLowerCase())
          );
        },
        command: ({ editor, range, props }) => {
          props.command({ editor, range });
        },
        render: () => {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          let component: any;
          let popup: TippyInstance[];

          return {
            onStart: (props: SuggestionProps) => {
              component = new ReactRenderer(CommandsList, {
                props,
                editor: props.editor,
              });

              if (!props.clientRect) {
                return;
              }

              popup = tippy('body', {
                getReferenceClientRect: () => props.clientRect?.() || new DOMRect(),
                appendTo: () => document.body,
                content: component.element,
                showOnCreate: true,
                interactive: true,
                trigger: 'manual',
                placement: 'bottom-start',
              });
            },

            onUpdate(props: SuggestionProps) {
              component.updateProps(props);

              if (!props.clientRect) {
                return;
              }

              popup[0].setProps({
                getReferenceClientRect: () => props.clientRect?.() || new DOMRect(),
              });
            },

            onKeyDown(props: { event: KeyboardEvent }) {
              if (props.event.key === 'Escape') {
                popup[0].hide();
                return true;
              }

              return component.ref?.onKeyDown(props);
            },

            onExit() {
              popup[0].destroy();
              component.destroy();
            },
          };
        },
      }),
    ],
    content: value,
    onUpdate: ({ editor }) => {
      const html = editor.getHTML();
      onChange(html);
    },
    editorProps: {
      attributes: {
        class: 'prose prose-sm max-w-none min-h-[400px] p-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white',
      },
    },
  });

  // Sync external value changes to editor
  useEffect(() => {
    if (editor && value !== editor.getHTML()) {
      editor.commands.setContent(value);
    }
  }, [value, editor]);

  const characterCount = editor?.storage.characterCount.characters() || 0;
  const isOverLimit = characterCount > maxCharacters;

  if (!editor) {
    return (
      <div className="flex items-center justify-center h-[400px] border border-gray-300 rounded-lg bg-gray-50">
        <div className="text-gray-500">エディターを読み込み中...</div>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label htmlFor="body">
          本文 <span className="text-red-500">*</span>
        </Label>
        <div className="flex items-center gap-4">
          <CharacterCounter count={characterCount} max={maxCharacters} />
          <Button
            type="button"
            onClick={onTogglePreview}
            variant="secondary"
            size="sm"
          >
            {showPreview ? '編集モード' : 'プレビュー'}
          </Button>
        </div>
      </div>
      
      {isOverLimit && (
        <p className="text-sm text-red-500">
          文字数が上限を超えています。{maxCharacters.toLocaleString()}文字以内に収めてください。
        </p>
      )}
      
      {showPreview ? (
        <div className="min-h-[400px] px-4 py-3 border border-gray-300 rounded-lg bg-white markdown-preview">
          <div dangerouslySetInnerHTML={{ __html: editor.getHTML() }} />
        </div>
      ) : (
        <EditorContent editor={editor} />
      )}
    </div>
  );
}
