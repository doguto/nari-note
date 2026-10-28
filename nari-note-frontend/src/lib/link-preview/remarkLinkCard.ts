import type { Paragraph, Root } from 'mdast';

export const LINK_CARD_ATTRIBUTE = 'data-link-card';

const URL_PATTERN = /^https?:\/\/[^\s<>"]+$/;

/**
 * URL だけの段落（前後が空行の URL 単独行）を、リンクカード用の div に変換する。
 * 描画側は data-link-card 属性を持つ div をリンクカードとして扱う。
 */
export function remarkLinkCard() {
  return (tree: Root) => {
    for (const node of tree.children) {
      if (node.type !== 'paragraph') continue;
      const url = getStandaloneUrl(node);
      if (!url) continue;
      node.data = { ...node.data, hName: 'div', hProperties: { [LINK_CARD_ATTRIBUTE]: url } };
    }
  };
}

function getStandaloneUrl(paragraph: Paragraph): string | null {
  if (paragraph.children.length !== 1) return null;
  const child = paragraph.children[0];

  if (child.type === 'text') {
    const text = child.value.trim();
    return URL_PATTERN.test(text) ? text : null;
  }

  // <https://...> の自動リンク記法
  if (child.type === 'link' && child.children.length === 1) {
    const label = child.children[0];
    if (label.type === 'text' && label.value === child.url && URL_PATTERN.test(child.url)) return child.url;
  }

  return null;
}
