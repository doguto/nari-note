// id はバックエンドの AnalysisEngine と同じ値にする
export const ANALYSIS_ENGINES = [
  { id: 'tsuyouraou-mini', label: 'つようら王11mini', description: '強い' },
  { id: 'tsuyouraou-micro', label: 'つようら王11micro', description: '軽い' },
] as const;

export type AnalysisEngineId = (typeof ANALYSIS_ENGINES)[number]['id'];

export const DEFAULT_ANALYSIS_ENGINE: AnalysisEngineId = 'tsuyouraou-mini';

export function isAnalysisEngineId(value: string | null): value is AnalysisEngineId {
  return ANALYSIS_ENGINES.some((e) => e.id === value);
}
