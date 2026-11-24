const MATE_THRESHOLD = 31000;

export function formatScore(score: number): string {
  if (score >= MATE_THRESHOLD) return '先手勝ち';
  if (score <= -MATE_THRESHOLD) return '後手勝ち';
  return score > 0 ? `+${score}` : String(score);
}
