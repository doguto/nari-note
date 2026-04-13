import { getCloudflareContext } from '@opennextjs/cloudflare';

export const getEnv = (key: string): string | undefined => {
  // 1. Cloudflare Workers環境での取得を試みる
  try {
    const cfContext = getCloudflareContext()
    if (cfContext && cfContext.env && (cfContext.env as any)[key]) {
      return (cfContext.env as any)[key]
    }
  } catch (e) {
    // ローカルの npm run dev など、Cloudflareコンテキストがない場合はこちら
  }
  // 2. フォールバックとして標準の process.env を見る
  // (ローカル開発時や、ビルド時に埋め込まれた変数はここにある)
  return process.env[key]
}
