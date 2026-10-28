import type { LinkPreview } from './types';

const MAX_TITLE_LENGTH = 200;
const MAX_DESCRIPTION_LENGTH = 300;
const MAX_SITE_NAME_LENGTH = 100;

const META_TAG_PATTERN = /<meta\s[^>]*>/gi;
const ATTRIBUTE_PATTERN = /([\w:-]+)\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s"'>]+))/g;
const TITLE_TAG_PATTERN = /<title[^>]*>([\s\S]*?)<\/title>/i;

const NAMED_ENTITIES: Record<string, string> = {
  amp: '&',
  lt: '<',
  gt: '>',
  quot: '"',
  apos: "'",
  nbsp: ' ',
};

export function parseOgp(html: string, pageUrl: URL): LinkPreview | null {
  // Next.js 15.2 以降は bot 以外の UA に metadata を body 側へストリーミングするため、meta はドキュメント全体から読む。
  // title タグは SVG の title 等を拾わないよう head に限定する
  const headEnd = html.search(/<\/head>/i);
  const head = headEnd >= 0 ? html.slice(0, headEnd) : html;

  const meta = parseMetaTags(html);
  const title = clean(
    meta.get('og:title') ?? meta.get('twitter:title') ?? head.match(TITLE_TAG_PATTERN)?.[1],
    MAX_TITLE_LENGTH,
  );
  if (!title) return null;

  return {
    title,
    description: clean(
      meta.get('og:description') ?? meta.get('twitter:description') ?? meta.get('description'),
      MAX_DESCRIPTION_LENGTH,
    ),
    imageUrl: resolveHttpUrl(meta.get('og:image') ?? meta.get('og:image:url') ?? meta.get('twitter:image'), pageUrl),
    siteName: clean(meta.get('og:site_name'), MAX_SITE_NAME_LENGTH),
  };
}

// 同じキーが複数ある場合は先頭を採用する
function parseMetaTags(head: string): Map<string, string> {
  const meta = new Map<string, string>();
  for (const [tag] of head.matchAll(META_TAG_PATTERN)) {
    const attributes = new Map<string, string>();
    for (const m of tag.matchAll(ATTRIBUTE_PATTERN)) {
      const name = m[1].toLowerCase();
      if (!attributes.has(name)) attributes.set(name, m[2] ?? m[3] ?? m[4]);
    }
    const key = (attributes.get('property') ?? attributes.get('name'))?.toLowerCase();
    const content = attributes.get('content');
    if (key && content !== undefined && !meta.has(key)) meta.set(key, content);
  }
  return meta;
}

function decodeEntities(value: string): string {
  return value.replace(/&(#x[0-9a-f]+|#\d+|[a-z]+);/gi, (entity, body: string) => {
    if (body[0] === '#') {
      const codePoint = body[1] === 'x' || body[1] === 'X' ? parseInt(body.slice(2), 16) : parseInt(body.slice(1), 10);
      return codePoint > 0 && codePoint <= 0x10ffff ? String.fromCodePoint(codePoint) : entity;
    }
    return NAMED_ENTITIES[body.toLowerCase()] ?? entity;
  });
}

function clean(value: string | undefined, maxLength: number): string | undefined {
  if (value === undefined) return undefined;
  const cleaned = decodeEntities(value).replace(/\s+/g, ' ').trim();
  if (!cleaned) return undefined;
  return cleaned.length > maxLength ? `${cleaned.slice(0, maxLength)}…` : cleaned;
}

function resolveHttpUrl(value: string | undefined, baseUrl: URL): string | undefined {
  if (!value?.trim()) return undefined;
  try {
    const url = new URL(decodeEntities(value).trim(), baseUrl);
    return url.protocol === 'http:' || url.protocol === 'https:' ? url.href : undefined;
  } catch {
    return undefined;
  }
}
