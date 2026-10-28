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
