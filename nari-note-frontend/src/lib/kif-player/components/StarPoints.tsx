import { cn } from '@/lib/utils';

const STAR_POSITIONS = ['left-1/3 top-1/3', 'left-2/3 top-1/3', 'left-1/3 top-2/3', 'left-2/3 top-2/3'];

/** 盤面の星（3・6筋と三・六段の交点）。position: relative な盤面の枠の中に置く */
export function StarPoints() {
  return (
    <div className="absolute inset-0 pointer-events-none">
      {STAR_POSITIONS.map((position) => (
        <span
          key={position}
          className={cn('absolute size-1.5 -translate-x-1/2 -translate-y-1/2 rounded-full bg-shogi-line', position)}
        />
      ))}
    </div>
  );
}
