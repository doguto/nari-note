#!/usr/bin/env python3
"""
駒の素材を生成する

- フロントエンド: 駒の文字をアウトライン化した SVG のパス（lib/kif-player/constants/komaGlyphPaths.ts）
- バックエンド: OG 画像の盤面に貼る駒の PNG（Src/Infrastructure/Asset/Koma/*.png、先手向き 44px）

駒の形・文字・配色はフロントエンドの定義（constants/index.ts、globals.css）から読み取り、
PNG はフロントエンドの KomaImage と同じ SVG を描いて作るため、両者の見た目は常に一致する
字体（Shippori Mincho）は Google Fonts から駒の文字だけのサブセットを取得する

使い方:
    pip install fonttools skia-python
    python scripts/generate_koma_assets.py
"""

import io
import re
import urllib.parse
import urllib.request
from pathlib import Path

import skia
from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont

REPO_ROOT = Path(__file__).parent.parent
FRONTEND_SRC = REPO_ROOT / "nari-note-frontend" / "src"
CONSTANTS_FILE = FRONTEND_SRC / "lib" / "kif-player" / "constants" / "index.ts"
GLOBALS_CSS = FRONTEND_SRC / "app" / "globals.css"
GLYPH_PATHS_FILE = FRONTEND_SRC / "lib" / "kif-player" / "constants" / "komaGlyphPaths.ts"
KOMA_IMAGE_DIR = REPO_ROOT / "nari-note-backend" / "Src" / "Infrastructure" / "Asset" / "Koma"

FONT_FAMILY = "Shippori Mincho"
FONT_WEIGHT = 700

# viewBox 0 0 60 60 での文字の大きさと、文字の中心（上下はアセンダーとディセンダーの中央）
VIEW_BOX = 60
GLYPH_SIZE = 30
GLYPH_CENTER_X = 30
GLYPH_CENTER_Y = 35

# バックエンドの PNG の大きさ。ArticleOgImageRenderer は等倍で貼る
PNG_SIZE = 44

# 駒の種類 → バックエンドの PNG のファイル名。KomaImageRepository.FileNames と揃える
PNG_FILE_NAMES = {
    "玉": "gyoku", "王": "ou", "飛": "hisha", "角": "kaku", "金": "kin",
    "銀": "gin", "桂": "keima", "香": "kyosha", "歩": "fu",
    "龍": "ryu", "馬": "uma", "成銀": "narigin", "成桂": "narikei", "成香": "narikyo", "と": "tokin",
}


def read_frontend_definitions() -> tuple[dict[str, str], set[str], str, dict[str, str]]:
    constants = CONSTANTS_FILE.read_text(encoding="utf-8")

    label_block = re.search(r"KOMA_LABEL_MAP[^{]*\{(.*?)\};", constants, re.S).group(1)
    labels = dict(re.findall(r"'(.+?)':\s*'(.+?)'", label_block))

    promoted_block = re.search(r"PROMOTED_PIECES[^\[]*\[(.*?)\]", constants, re.S).group(1)
    promoted = set(re.findall(r"'(.+?)'", promoted_block))

    shape = re.search(r"KOMA_SHAPE_POINTS\s*=\s*'(.+?)'", constants).group(1)

    css = GLOBALS_CSS.read_text(encoding="utf-8")
    colors = dict(re.findall(r"--color-shogi-([\w-]+):\s*(#[0-9a-fA-F]{6});", css))

    return labels, promoted, shape, colors


# User-Agent を付けないと Google Fonts は woff2 ではなく TrueType を返す
def load_font(text: str) -> TTFont:
    query = urllib.parse.urlencode({"family": f"{FONT_FAMILY}:wght@{FONT_WEIGHT}", "text": text}, safe=":@")
    with urllib.request.urlopen(f"https://fonts.googleapis.com/css2?{query}") as res:
        css = res.read().decode()

    match = re.search(r"src: url\((.+?)\) format\('(?:opentype|truetype)'\)", css)
    if match is None:
        raise RuntimeError(f"Font url not found in css: {css}")

    with urllib.request.urlopen(match.group(1)) as res:
        return TTFont(io.BytesIO(res.read()))


def glyph_path(font: TTFont, char: str) -> str:
    glyph_name = font.getBestCmap()[ord(char)]
    glyph_set = font.getGlyphSet()
    advance = glyph_set[glyph_name].width

    scale = GLYPH_SIZE / font["head"].unitsPerEm
    hhea = font["hhea"]
    baseline = GLYPH_CENTER_Y + (hhea.ascent + hhea.descent) / 2 * scale
    left = GLYPH_CENTER_X - advance * scale / 2

    # フォントの座標は y が上向きなので反転する
    pen = SVGPathPen(glyph_set, ntos=lambda v: f"{v:.2f}".rstrip("0").rstrip("."))
    glyph_set[glyph_name].draw(TransformPen(pen, (scale, 0, 0, -scale, left, baseline)))
    return pen.getCommands()


def koma_svg(shape: str, glyph: str, fill: str, colors: dict[str, str]) -> str:
    # フロントエンドの KomaImage と同じ構成
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{PNG_SIZE}" height="{PNG_SIZE}" viewBox="0 0 {VIEW_BOX} {VIEW_BOX}">'
        f'<polygon points="{shape}" fill="{colors["koma"]}" stroke="{colors["koma-edge"]}" stroke-width="2.5" stroke-linejoin="round"/>'
        f'<path d="{glyph}" fill="{fill}"/>'
        "</svg>"
    )


def render_png(svg: str, path: Path) -> None:
    # MemoryStream はデータをコピーしないため、描き終えるまで data を保持する
    data = skia.Data.MakeWithCopy(svg.encode())
    dom = skia.SVGDOM.MakeFromStream(skia.MemoryStream(data))
    dom.setContainerSize(skia.Size(PNG_SIZE, PNG_SIZE))
    surface = skia.Surface(PNG_SIZE, PNG_SIZE)
    canvas = surface.getCanvas()
    canvas.clear(skia.ColorTRANSPARENT)
    dom.render(canvas)
    surface.makeImageSnapshot().save(str(path), skia.kPNG)


def write_glyph_paths(paths: dict[str, str]) -> None:
    entries = "\n".join(f"  '{piece}': '{d}'," for piece, d in paths.items())
    GLYPH_PATHS_FILE.write_text(
        "// このファイルは scripts/generate_koma_assets.py で生成する。直接編集しない\n"
        "import type { PieceType } from '../types';\n"
        "\n"
        f"// 駒に書く文字（KOMA_LABEL_MAP）を {FONT_FAMILY} でアウトライン化したパス（viewBox 0 0 {VIEW_BOX} {VIEW_BOX}）\n"
        "export const KOMA_GLYPH_PATHS: Record<PieceType, string> = {\n"
        f"{entries}\n"
        "};\n",
        encoding="utf-8",
    )


def main() -> None:
    labels, promoted, shape, colors = read_frontend_definitions()
    font = load_font("".join(sorted(set(labels.values()))))

    paths = {piece: glyph_path(font, label) for piece, label in labels.items()}
    write_glyph_paths(paths)
    print(f"generated {GLYPH_PATHS_FILE.relative_to(REPO_ROOT)}")

    KOMA_IMAGE_DIR.mkdir(parents=True, exist_ok=True)
    for piece, d in paths.items():
        fill = colors["promoted"] if piece in promoted else colors["ink"]
        render_png(koma_svg(shape, d, fill, colors), KOMA_IMAGE_DIR / f"{PNG_FILE_NAMES[piece]}.png")
    print(f"generated {len(paths)} images in {KOMA_IMAGE_DIR.relative_to(REPO_ROOT)}")


if __name__ == "__main__":
    main()
