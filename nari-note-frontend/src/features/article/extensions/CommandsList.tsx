'use client';

import { forwardRef, useEffect, useImperativeHandle, useState } from 'react';
import { Command } from './SlashCommand';
import {
  Heading1,
  Heading2,
  Heading3,
  List,
  ListOrdered,
  Code,
  Quote,
  Minus,
  LucideIcon,
} from 'lucide-react';

interface CommandsListProps {
  items: Command[];
  command: (item: Command) => void;
}

interface KeyDownHandler {
  onKeyDown: ({ event }: { event: KeyboardEvent }) => boolean;
}

const iconMap: Record<string, LucideIcon> = {
  Heading1,
  Heading2,
  Heading3,
  List,
  ListOrdered,
  Code,
  Quote,
  Minus,
};

export const CommandsList = forwardRef<KeyDownHandler, CommandsListProps>((props, ref) => {
  const [selectedIndex, setSelectedIndex] = useState(0);

  const selectItem = (index: number) => {
    const item = props.items[index];
    if (item) {
      props.command(item);
    }
  };

  const upHandler = () => {
    setSelectedIndex((selectedIndex + props.items.length - 1) % props.items.length);
  };

  const downHandler = () => {
    setSelectedIndex((selectedIndex + 1) % props.items.length);
  };

  const enterHandler = () => {
    selectItem(selectedIndex);
  };

  useEffect(() => {
    setSelectedIndex(0);
  }, [props.items]);

  useImperativeHandle(ref, () => ({
    onKeyDown: ({ event }: { event: KeyboardEvent }) => {
      if (event.key === 'ArrowUp') {
        upHandler();
        return true;
      }

      if (event.key === 'ArrowDown') {
        downHandler();
        return true;
      }

      if (event.key === 'Enter') {
        enterHandler();
        return true;
      }

      return false;
    },
  }));

  if (props.items.length === 0) {
    return (
      <div className="w-80 bg-white border border-gray-200 rounded-lg shadow-lg p-4">
        <div className="text-center text-gray-500 text-sm">
          該当するコマンドが見つかりません
        </div>
      </div>
    );
  }

  return (
    <div className="w-80 bg-white border border-gray-200 rounded-lg shadow-lg max-h-96 overflow-y-auto">
      {props.items.map((item, index) => {
        const Icon = iconMap[item.icon];
        return (
          <button
            key={item.id}
            onClick={() => selectItem(index)}
            className={`w-full flex items-start gap-3 px-4 py-3 text-left transition-colors ${
              index === selectedIndex
                ? 'bg-blue-50'
                : 'hover:bg-gray-50'
            }`}
          >
            {Icon && <Icon className="w-5 h-5 mt-0.5 flex-shrink-0 text-gray-600" />}
            <div className="flex-1 min-w-0">
              <div className="font-medium text-gray-900">{item.title}</div>
              <div className="text-sm text-gray-500 truncate">{item.description}</div>
            </div>
          </button>
        );
      })}
    </div>
  );
});

CommandsList.displayName = 'CommandsList';
