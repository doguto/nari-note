import { getCloudflareContext } from '@opennextjs/cloudflare';

interface AssetsBinding {
  fetch(input: URL): Promise<Response>;
}

/**
 * public 配下のファイルを読む。
 * Workers 上では自ドメインへの fetch が Worker 自身に届かない場合があるため、静的アセットのバインディング（wrangler.toml の ASSETS）から読む
 */
export async function loadPublicAsset(path: string, requestUrl: string): Promise<ArrayBuffer | null> {
  const url = new URL(path, requestUrl);
  const assets = getAssetsBinding();
  const response = await (assets ? assets.fetch(url) : fetch(url));
  return response.ok ? response.arrayBuffer() : null;
}

function getAssetsBinding(): AssetsBinding | undefined {
  try {
    return (getCloudflareContext().env as { ASSETS?: AssetsBinding }).ASSETS;
  } catch {
    // next dev（Node.js）では Cloudflare のコンテキストが無い
    return undefined;
  }
}

export function toDataUri(data: ArrayBuffer, mimeType: string): string {
  return `data:${mimeType};base64,${Buffer.from(data).toString('base64')}`;
}

/**
 * 描画する文字だけを含む Google Fonts のサブセットを読む（satori は woff2 を読めないため truetype を取得する）
 */
export async function loadGoogleFont(family: string, weight: number, text: string): Promise<ArrayBuffer | null> {
  const cssUrl = `https://fonts.googleapis.com/css2?family=${encodeURIComponent(family)}:wght@${weight}&text=${encodeURIComponent(text)}`;
  const css = await (await fetch(cssUrl)).text();
  const fontUrl = css.match(/src: url\((.+?)\) format\('(?:opentype|truetype)'\)/)?.[1];
  if (!fontUrl) return null;
  const response = await fetch(fontUrl);
  return response.ok ? response.arrayBuffer() : null;
}
