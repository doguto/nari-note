'use client';

import { Extension } from '@tiptap/core';
import { Editor, Range } from '@tiptap/react';
import Suggestion, { SuggestionOptions } from '@tiptap/suggestion';

export interface Command {
  id: string;
  title: string;
  description: string;
  icon: string;
  command: ({ editor, range }: { editor: Editor; range: Range }) => void;
}

export const commands: Command[] = [
  {
    id: 'h1',
    title: '見出し1',
    description: '大見出しを挿入',
    icon: 'Heading1',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .setNode('heading', { level: 1 })
        .run();
    },
  },
  {
    id: 'h2',
    title: '見出し2',
    description: '中見出しを挿入',
    icon: 'Heading2',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .setNode('heading', { level: 2 })
        .run();
    },
  },
  {
    id: 'h3',
    title: '見出し3',
    description: '小見出しを挿入',
    icon: 'Heading3',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .setNode('heading', { level: 3 })
        .run();
    },
  },
  {
    id: 'ul',
    title: '箇条書きリスト',
    description: '箇条書きリストを挿入',
    icon: 'List',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .toggleBulletList()
        .run();
    },
  },
  {
    id: 'ol',
    title: '番号付きリスト',
    description: '番号付きリストを挿入',
    icon: 'ListOrdered',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .toggleOrderedList()
        .run();
    },
  },
  {
    id: 'code',
    title: 'コードブロック',
    description: 'コードブロックを挿入',
    icon: 'Code',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .toggleCodeBlock()
        .run();
    },
  },
  {
    id: 'quote',
    title: '引用',
    description: '引用ブロックを挿入',
    icon: 'Quote',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .toggleBlockquote()
        .run();
    },
  },
  {
    id: 'hr',
    title: '水平線',
    description: '水平線を挿入',
    icon: 'Minus',
    command: ({ editor, range }) => {
      editor
        .chain()
        .focus()
        .deleteRange(range)
        .setHorizontalRule()
        .run();
    },
  },
];

export function createSlashCommandExtension(suggestionOptions: Partial<SuggestionOptions>) {
  return Extension.create({
    name: 'slashCommand',

    addProseMirrorPlugins() {
      return [
        Suggestion({
          editor: this.editor,
          ...suggestionOptions,
        }),
      ];
    },
  });
}
