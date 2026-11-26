import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface ResearchPanelProps {
  title: string;
  icon?: ReactNode;
  note?: ReactNode;
  plain?: boolean;
  className?: string;
  children: ReactNode;
}

// plain はタブの中など、外側に枠がある場所で使う
export function ResearchPanel({ title, icon, note, plain = false, className, children }: ResearchPanelProps) {
  return (
    <section
      className={cn(
        'flex flex-col gap-3',
        !plain && 'rounded-xl border border-brand-border bg-white p-4 shadow-sm',
        className,
      )}
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="flex items-center gap-2 text-sm font-bold text-brand-text">
          {icon && <span className="text-brand-primary-dark">{icon}</span>}
          {title}
        </h2>
        {note && <span className="text-xs text-brand-secondary-text">{note}</span>}
      </div>
      {children}
    </section>
  );
}
