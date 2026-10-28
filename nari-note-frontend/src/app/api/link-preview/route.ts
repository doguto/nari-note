import { getCloudflareContext } from '@opennextjs/cloudflare';
import { type NextRequest, NextResponse } from 'next/server';
import { parseOgp } from '@/lib/link-preview/parseOgp';
import type { LinkPreviewResponse } from '@/lib/link-preview/types';

// /api/[...path] のバックエンドプロキシより優先され、ここだけは Next.js 側で処理する。
// Cloudflare Workers 上で動くため、取得先から AWS 内部（VPC・EC2 メタデータ）には到達しない。

const FETCH_TIMEOUT_MS = 5000;
// OGP は head 内にあるため先頭だけ読めば足りる（YouTube は head 内の og:title が先頭から約 750KB の位置にある）
const MAX_HTML_BYTES = 2 * 1024 * 1024;
const SUCCESS_CACHE_CONTROL = 'public, max-age=3600, s-maxage=86400';
const FAILURE_CACHE_CONTROL = 'public, max-age=600, s-maxage=600';
const USER_AGENT = 'nari-note-link-preview/1.0';

export async function GET(request: NextRequest) {
  const target = parseTargetUrl(request.nextUrl.searchParams.get('url'));
  if (!target) {
    return NextResponse.json({ message: 'URLの形式が正しくありません。' }, { status: 400 });
  }

  const cache = getEdgeCache();
  const cacheKey = new URL(request.nextUrl.pathname, request.nextUrl.origin);
  cacheKey.searchParams.set('url', target.href);
  const cached = await cache?.match(cacheKey);
  if (cached) return cached;

  const body = await fetchPreview(target, target.host === request.nextUrl.host ? getSelfReference() : undefined);
  const succeeded = body.title !== undefined || body.isImage === true;
  const response = NextResponse.json(body, {
    headers: { 'Cache-Control': succeeded ? SUCCESS_CACHE_CONTROL : FAILURE_CACHE_CONTROL },
  });
  await cache?.put(cacheKey, response.clone());
  return response;
}

function parseTargetUrl(value: string | null): URL | null {
  if (!value || value.length > 2048) return null;
  let url: URL;
  try {
    url = new URL(value);
  } catch {
    return null;
  }
  if (url.protocol !== 'http:' && url.protocol !== 'https:') return null;
  if (url.port !== '' || url.username !== '' || url.password !== '') return null;
  // IP 直指定や localhost はカード化する意味が無く、ローカル実行時に内部へ届くのを避けるため拒否する
  if (url.hostname === 'localhost' || /^[\d.]+$/.test(url.hostname) || url.hostname.startsWith('[')) return null;
  return url;
}

// Cloudflare Workers の Cache API。next dev（Node.js）では存在しないため無ければキャッシュしない
function getEdgeCache(): Cache | undefined {
  return (globalThis as { caches?: { default?: Cache } }).caches?.default;
}

interface ServiceBinding {
  fetch(input: URL, init?: RequestInit): Promise<Response>;
}

// Workers 上では自ドメインへの fetch が Worker 自身に届かないため、自サイトのページはサービスバインディング（wrangler.toml の WORKER_SELF_REFERENCE）経由で読む
function getSelfReference(): ServiceBinding | undefined {
  try {
    return (getCloudflareContext().env as { WORKER_SELF_REFERENCE?: ServiceBinding }).WORKER_SELF_REFERENCE;
  } catch {
    // next dev（Node.js）では Cloudflare のコンテキストが無い
    return undefined;
  }
}

async function fetchPreview(url: URL, fetcher?: ServiceBinding): Promise<LinkPreviewResponse> {
  const empty: LinkPreviewResponse = { url: url.href };
  try {
    const response = await (fetcher ? fetcher.fetch.bind(fetcher) : fetch)(url, {
      headers: { Accept: 'text/html,application/xhtml+xml', 'User-Agent': USER_AGENT },
      redirect: 'follow',
      signal: AbortSignal.timeout(FETCH_TIMEOUT_MS),
    });
    if (!response.ok || !response.body) return empty;

    const contentType = response.headers.get('content-type') ?? '';
    if (!/^(text\/html|application\/xhtml\+xml)/i.test(contentType)) {
      await response.body.cancel().catch(() => undefined);
      return /^image\//i.test(contentType) ? { ...empty, isImage: true } : empty;
    }

    const bytes = await readLimited(response.body, MAX_HTML_BYTES);
    const html = decode(bytes, contentType);
    return { ...empty, ...parseOgp(html, new URL(response.url || url)) };
  } catch (error) {
    console.info('[link-preview] fetch failed', url.href, error);
    return empty;
  }
}

async function readLimited(stream: ReadableStream<Uint8Array>, maxBytes: number): Promise<Uint8Array> {
  const reader = stream.getReader();
  const buffer = new Uint8Array(maxBytes);
  let total = 0;
  while (total < maxBytes) {
    const { done, value } = await reader.read();
    if (done) break;
    const chunk = value.subarray(0, maxBytes - total);
    buffer.set(chunk, total);
    total += chunk.length;
  }
  await reader.cancel().catch(() => undefined);
  return buffer.subarray(0, total);
}

function decode(bytes: Uint8Array, contentType: string): string {
  const charset =
    contentType.match(/charset=["']?([\w-]+)/i)?.[1] ??
    new TextDecoder('ascii').decode(bytes.subarray(0, 2048)).match(/charset\s*=\s*["']?([\w-]+)/i)?.[1] ??
    'utf-8';
  try {
    return new TextDecoder(charset).decode(bytes);
  } catch {
    // 実行環境が対応していない文字コードは UTF-8 として読む
    return new TextDecoder().decode(bytes);
  }
}
