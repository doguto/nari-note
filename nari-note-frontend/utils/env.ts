import { getCloudflareContext } from '@opennextjs/cloudflare';

export const getEnv = (key: string): string | undefined => {
  // 1. Cloudflare Workers環境での取得を試みる
  try {
    const cfContext = getCloudflareContext()
    const value = (cfContext?.env as Record<string, unknown> | undefined)?.[key]
    if (typeof value === 'string' && value) {
      return value
    }
  } catch {
    // ローカルの npm run dev など、Cloudflareコンテキストがない場合はこちら
  }
  // 2. フォールバックとして標準の process.env を見る
  // (ローカル開発時や、ビルド時に埋め込まれた変数はここにある)
  return process.env[key]
}
