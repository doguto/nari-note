import { useSyncExternalStore } from 'react';

const subscribe = () => () => {};

/**
 * クライアントでのハイドレーション完了後に true を返す
 * サーバーサイドとクライアントサイドの初期レンダリングを一致させるために使用する
 */
export function useIsMounted(): boolean {
  return useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  );
}
