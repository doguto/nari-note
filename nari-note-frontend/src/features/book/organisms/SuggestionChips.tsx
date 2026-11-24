'use client';

interface SuggestionChipsProps {
  label: string;
  suggestions: string[];
  disabled?: boolean;
  onPick: (value: string) => void;
}

export function SuggestionChips({ label, suggestions, disabled, onPick }: SuggestionChipsProps) {
  if (suggestions.length === 0) return null;

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      <span className="text-xs text-brand-secondary-text">{label}</span>
      {suggestions.map((s) => (
        <button
          key={s}
          type="button"
          disabled={disabled}
          className="rounded-full border border-dashed border-brand-border px-2.5 py-0.5 text-xs text-brand-text hover:border-brand-primary-dark hover:bg-brand-primary-light disabled:opacity-50"
          onClick={() => onPick(s)}
        >
          {s}
        </button>
      ))}
    </div>
  );
}
