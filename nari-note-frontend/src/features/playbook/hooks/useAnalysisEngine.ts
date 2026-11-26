'use client';

import { useCallback, useSyncExternalStore } from 'react';
import { type AnalysisEngineId, DEFAULT_ANALYSIS_ENGINE, isAnalysisEngineId } from '../utils/analysisEngine';

const STORAGE_KEY = 'nari-note:analysis-engine';
const listeners = new Set<() => void>();

function subscribe(listener: () => void) {
  listeners.add(listener);
  window.addEventListener('storage', listener);
  return () => {
    listeners.delete(listener);
    window.removeEventListener('storage', listener);
  };
}

// localStorage を使えない環境でも、開いている間は切り替えられるようにする
let selected: AnalysisEngineId | null = null;

function read(): AnalysisEngineId {
  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    if (isAnalysisEngineId(stored)) return stored;
  } catch {}
  return selected ?? DEFAULT_ANALYSIS_ENGINE;
}

// 研究盤を開き直しても同じエンジンで解析できるよう、選んだエンジンをブラウザに覚えておく
export function useAnalysisEngine(): [AnalysisEngineId, (engine: AnalysisEngineId) => void] {
  const engine = useSyncExternalStore(subscribe, read, () => DEFAULT_ANALYSIS_ENGINE);

  const setEngine = useCallback((next: AnalysisEngineId) => {
    selected = next;
    try {
      window.localStorage.setItem(STORAGE_KEY, next);
    } catch {}
    listeners.forEach((l) => l());
  }, []);

  return [engine, setEngine];
}
