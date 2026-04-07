'use client';

import { useEditor, EditorContent, Extension } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Placeholder from '@tiptap/extension-placeholder';
import CharacterCount from '@tiptap/extension-character-count';
import { useEffect } from 'react';

interface TipTapEditorProps {
  content: string;
  onUpdate: (content: string) => void;
  maxCharacters: number;
  placeholder?: string;
  extensions?: Extension[];
}

/**
 * TipTapEditor - Core TipTap editor component
 */
export function TipTapEditor({
  content,
  onUpdate,
  maxCharacters,
  placeholder = '本文を入力してください...',
  extensions = [],
}: TipTapEditorProps) {
  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        heading: {
          levels: [1, 2, 3],
        },
      }),
      Placeholder.configure({
        placeholder,
      }),
      CharacterCount.configure({
        limit: maxCharacters,
      }),
      ...extensions,
    ],
    content,
    onUpdate: ({ editor }) => {
      const html = editor.getHTML();
      onUpdate(html);
    },
    editorProps: {
      attributes: {
        class: 'prose prose-sm max-w-none min-h-[40vh] p-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500',
      },
    },
  });

  // Update editor content when prop changes
  useEffect(() => {
    if (editor && content !== editor.getText()) {
      editor.commands.setContent(content);
    }
  }, [content, editor]);

  if (!editor) {
    return (
      <div className="flex items-center justify-center min-h-[40vh] border border-gray-300 rounded-lg bg-gray-50">
        <div className="text-gray-500">エディターを読み込み中...</div>
      </div>
    );
  }

  return (
    <div className="tiptap-editor">
      <EditorContent editor={editor} />
    </div>
  );
}
