'use client';

import { useState, useRef, useEffect, KeyboardEvent } from 'react';
import { Label } from '@/components/ui/label';
import { CharacterCounter, NarinoteMarkdown } from '@/components/molecules';

const PREVIEW_PLACEHOLDER = '*プレビューがここに表示されます*';

interface MarkdownEditorProps {
  value: string;
  onChange: (value: string) => void;
  maxCharacters?: number;
  placeholder?: string;
}

interface Command {
  trigger: string;
  label: string;
  insert: string;
  description: string;
}

// BOD形式の平手初期配置テンプレート
const BOARD_TEMPLATE = `\`\`\`bod
後手の持駒：なし
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
|v香v桂v銀v金v玉v金v銀v桂v香|一
| ・v飛 ・ ・ ・ ・ ・v角 ・|二
|v歩v歩v歩v歩v歩v歩v歩v歩v歩|三
| ・ ・ ・ ・ ・ ・ ・ ・ ・|四
| ・ ・ ・ ・ ・ ・ ・ ・ ・|五
| ・ ・ ・ ・ ・ ・ ・ ・ ・|六
| 歩 歩 歩 歩 歩 歩 歩 歩 歩|七
| ・ 角 ・ ・ ・ ・ ・ 飛 ・|八
| 香 桂 銀 金 玉 金 銀 桂 香|九
+---------------------------+
先手の持駒：なし
\`\`\``;

// BOD形式の空盤面テンプレート
const BOARD_EMPTY_TEMPLATE = `\`\`\`bod
後手の持駒：なし
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
| ・ ・ ・ ・ ・ ・ ・ ・ ・|一
| ・ ・ ・ ・ ・ ・ ・ ・ ・|二
| ・ ・ ・ ・ ・ ・ ・ ・ ・|三
| ・ ・ ・ ・ ・ ・ ・ ・ ・|四
| ・ ・ ・ ・ ・ ・ ・ ・ ・|五
| ・ ・ ・ ・ ・ ・ ・ ・ ・|六
| ・ ・ ・ ・ ・ ・ ・ ・ ・|七
| ・ ・ ・ ・ ・ ・ ・ ・ ・|八
| ・ ・ ・ ・ ・ ・ ・ ・ ・|九
+---------------------------+
先手の持駒：なし
\`\`\``;

const MARKDOWN_COMMANDS: Command[] = [
  { trigger: 'h1', label: '# 見出し1', insert: '# ', description: '大見出しを挿入' },
  { trigger: 'h2', label: '## 見出し2', insert: '## ', description: '中見出しを挿入' },
  { trigger: 'h3', label: '### 見出し3', insert: '### ', description: '小見出しを挿入' },
  { trigger: 'ul', label: '- 箇条書き', insert: '- ', description: '箇条書きリストを挿入' },
  { trigger: 'ol', label: '1. 番号付き', insert: '1. ', description: '番号付きリストを挿入' },
  { trigger: 'code', label: '``` コード', insert: '```\n\n```', description: 'コードブロックを挿入' },
  { trigger: 'quote', label: '> 引用', insert: '> ', description: '引用ブロックを挿入' },
  { trigger: 'hr', label: '--- 水平線', insert: '\n---\n', description: '水平線を挿入' },
  { trigger: 'board', label: '🎮 将棋盤面', insert: `\n${BOARD_TEMPLATE}\n`, description: '将棋盤面を挿入（平手初期配置）' },
  { trigger: 'board-empty', label: '🎮 空の盤面', insert: `\n${BOARD_EMPTY_TEMPLATE}\n`, description: '空の将棋盤面を挿入' },
];

/**
 * MarkdownEditor - Molecule Component
 * 
 * Markdown記法で入力し、リアルタイムでレンダリング表示するエディタ
 * スラッシュコマンドでMarkdown記法を簡単に挿入可能
 */
export function MarkdownEditor({
  value,
  onChange,
  maxCharacters = 65535,
  placeholder = '本文を入力してください... 「/」でコマンドメニューを開きます',
}: MarkdownEditorProps) {
  const [showCommands, setShowCommands] = useState(false);
  const [commandFilter, setCommandFilter] = useState('');
  const [selectedCommandIndex, setSelectedCommandIndex] = useState(0);
  const [commandMenuPosition, setCommandMenuPosition] = useState({ top: 0, left: 0 });
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const commandsRef = useRef<HTMLDivElement>(null);

  const characterCount = value.length;
  const isOverLimit = characterCount > maxCharacters;

  // Filter commands based on search query
  const filteredCommands = MARKDOWN_COMMANDS.filter(cmd =>
    cmd.trigger.toLowerCase().includes(commandFilter.toLowerCase()) ||
    cmd.label.toLowerCase().includes(commandFilter.toLowerCase()) ||
    cmd.description.toLowerCase().includes(commandFilter.toLowerCase())
  );

  // Calculate cursor position in pixels
  const getCursorPosition = (textarea: HTMLTextAreaElement, cursorPos: number) => {
    const textBeforeCursor = textarea.value.substring(0, cursorPos);
    const lines = textBeforeCursor.split('\n');
    const currentLineIndex = lines.length - 1;
    const currentLineText = lines[currentLineIndex];
    
    // Create a temporary element to measure text width
    const tempSpan = document.createElement('span');
    tempSpan.style.font = window.getComputedStyle(textarea).font;
    tempSpan.style.whiteSpace = 'pre';
    tempSpan.style.visibility = 'hidden';
    tempSpan.style.position = 'absolute';
    tempSpan.textContent = currentLineText;
    document.body.appendChild(tempSpan);
    
    const textWidth = tempSpan.offsetWidth;
    document.body.removeChild(tempSpan);
    
    // Calculate line height
    const lineHeight = parseInt(window.getComputedStyle(textarea).lineHeight) || 20;
    
    // Get textarea padding
    const paddingLeft = parseInt(window.getComputedStyle(textarea).paddingLeft) || 0;
    const paddingTop = parseInt(window.getComputedStyle(textarea).paddingTop) || 0;
    
    return {
      top: paddingTop + currentLineIndex * lineHeight + lineHeight,
      left: paddingLeft + textWidth,
    };
  };

  // Handle text change
  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = e.target.value;
    const cursorPos = e.target.selectionStart || 0;
    
    onChange(newValue);

    // Check for slash command
    const textBeforeCursor = newValue.substring(0, cursorPos);
    const lastSlashIndex = textBeforeCursor.lastIndexOf('/');
    
    if (lastSlashIndex !== -1) {
      const textAfterSlash = textBeforeCursor.substring(lastSlashIndex + 1);
      const hasSpace = textAfterSlash.includes(' ') || textAfterSlash.includes('\n');
      
      if (!hasSpace && (lastSlashIndex === 0 || newValue[lastSlashIndex - 1] === '\n' || newValue[lastSlashIndex - 1] === ' ')) {
        // Calculate the position of the slash character
        if (textareaRef.current) {
          const position = getCursorPosition(textareaRef.current, lastSlashIndex);
          setCommandMenuPosition(position);
        }
        setShowCommands(true);
        setCommandFilter(textAfterSlash);
        setSelectedCommandIndex(0);
      } else {
        setShowCommands(false);
      }
    } else {
      setShowCommands(false);
    }
  };

  // Insert command at cursor position
  const insertCommand = (command: Command) => {
    if (!textareaRef.current) return;

    const textarea = textareaRef.current;
    const cursorPos = textarea.selectionStart || 0;
    const textBeforeCursor = value.substring(0, cursorPos);
    const textAfterCursor = value.substring(cursorPos);
    
    // Find and remove the slash command trigger
    const lastSlashIndex = textBeforeCursor.lastIndexOf('/');
    const beforeSlash = value.substring(0, lastSlashIndex);
    
    // Insert the markdown syntax
    const newValue = beforeSlash + command.insert + textAfterCursor;
    onChange(newValue);
    
    setShowCommands(false);
    setCommandFilter('');
    
    // Set cursor position after inserted text
    setTimeout(() => {
      const newCursorPos = beforeSlash.length + command.insert.length;
      // For code blocks, position cursor between the opening and closing fences
      if (command.trigger === 'code') {
        const cursorPosInsideCodeBlock = beforeSlash.length + command.insert.indexOf('\n') + 1;
        textarea.setSelectionRange(cursorPosInsideCodeBlock, cursorPosInsideCodeBlock);
      } else {
        textarea.setSelectionRange(newCursorPos, newCursorPos);
      }
      textarea.focus();
    }, 0);
  };

  // Handle keyboard navigation in command menu
  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (!showCommands) return;

    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setSelectedCommandIndex(prev => 
        prev < filteredCommands.length - 1 ? prev + 1 : prev
      );
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setSelectedCommandIndex(prev => prev > 0 ? prev - 1 : prev);
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (filteredCommands[selectedCommandIndex]) {
        insertCommand(filteredCommands[selectedCommandIndex]);
      }
    } else if (e.key === 'Escape') {
      e.preventDefault();
      setShowCommands(false);
    }
  };

  // Handle tab key for indentation
  const handleTab = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Tab' && !showCommands) {
      e.preventDefault();
      const textarea = textareaRef.current;
      if (!textarea) return;

      const start = textarea.selectionStart;
      const end = textarea.selectionEnd;
      const newValue = value.substring(0, start) + '  ' + value.substring(end);
      
      onChange(newValue);
      
      setTimeout(() => {
        textarea.setSelectionRange(start + 2, start + 2);
      }, 0);
    }
  };

  // Auto-scroll selected command into view
  useEffect(() => {
    if (showCommands && commandsRef.current) {
      const selectedElement = commandsRef.current.children[selectedCommandIndex] as HTMLElement;
      if (selectedElement) {
        selectedElement.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
      }
    }
  }, [selectedCommandIndex, showCommands]);

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label htmlFor="markdown-editor">
          本文 <span className="text-red-500">*</span>
        </Label>
        <CharacterCounter count={characterCount} max={maxCharacters} />
      </div>

      {isOverLimit && (
        <p className="text-sm text-red-500">
          文字数が上限を超えています。{maxCharacters.toLocaleString()}文字以内に収めてください。
        </p>
      )}

      <div className="grid grid-cols-2 gap-4">
        {/* Editor Input */}
        <div className="relative">
          <textarea
            ref={textareaRef}
            id="markdown-editor"
            value={value}
            onChange={handleChange}
            onKeyDown={(e) => {
              handleKeyDown(e);
              handleTab(e);
            }}
            placeholder={placeholder}
            className="w-full min-h-[50vh] p-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white font-mono text-sm resize-y"
          />

          {/* Command Menu */}
          {showCommands && filteredCommands.length > 0 && (
            <div
              ref={commandsRef}
              className="absolute z-50 bg-white border border-gray-300 rounded-lg shadow-lg max-h-64 overflow-y-auto"
              style={{
                top: `${commandMenuPosition.top}px`,
                left: `${commandMenuPosition.left}px`,
                width: '80%',
              }}
            >
              {filteredCommands.map((cmd, index) => (
                <button
                  key={cmd.trigger}
                  type="button"
                  onClick={() => insertCommand(cmd)}
                  className={`w-full text-left px-4 py-2 hover:bg-gray-100 transition-colors ${
                    index === selectedCommandIndex ? 'bg-blue-50' : ''
                  }`}
                >
                  <div className="font-medium text-sm">{cmd.label}</div>
                  <div className="text-xs text-gray-500">{cmd.description}</div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Live Preview */}
        <div className="border border-gray-300 rounded-lg p-4 bg-white overflow-y-auto min-h-[50vh]">
          <div className="prose prose-sm max-w-none">
            <NarinoteMarkdown content={value || PREVIEW_PLACEHOLDER} />
          </div>
        </div>
      </div>

      <p className="text-xs text-gray-500">
        💡 Markdown記法を直接入力できます（例: <code className="bg-gray-100 px-1 rounded">## 見出し</code>）。
        または <code className="bg-gray-100 px-1 rounded">/</code> を入力してコマンドメニューを開いてください。
      </p>
    </div>
  );
}
