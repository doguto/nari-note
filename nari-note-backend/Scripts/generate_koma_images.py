"""
OG 画像の盤面に置く駒の画像（先手向き）を生成する

駒の形と配色はフロントエンドの盤面（globals.css の --color-shogi-*、lib/kif-player/constants の KOMA_SHAPE_POINTS）に揃える
後手の駒は ArticleOgImageRenderer で 180 度回転して描くため、先手向きの画像だけを作る
字体（Shippori Mincho）は Google Fonts から駒の文字だけのサブセットを取得する

使い方:
    pip install skia-python
    python Scripts/generate_koma_images.py
"""

import re
import urllib.parse
import urllib.request
from pathlib import Path

import skia

BACKEND_ROOT = Path(__file__).parent.parent
OUTPUT_DIR = BACKEND_ROOT / "Src" / "Application" / "Asset" / "Koma"

# ArticleOgImageRenderer.PieceSize と同じ値にする（等倍で貼るため）
PIECE_SIZE = 44
FONT_SIZE = 22
FONT_FAMILY = "Shippori Mincho"
FONT_WEIGHT = 700

KOMA = skia.Color(0xF6, 0xE2, 0xB3)
KOMA_EDGE = skia.Color(0x8A, 0x6A, 0x3A)
KOMA_INK = skia.Color(0x2A, 0x1D, 0x0C)
KOMA_PROMOTED = skia.Color(0xB3, 0x26, 0x1E)

# viewBox 0 0 60 60 の五角形（先手向き）
KOMA_SHAPE = [(30, 5), (49, 12), (54, 56), (6, 56), (11, 12)]

# ファイル名 → (駒に書く文字, 成駒か)。ファイル名は KomaImages.FileNames と揃える
PIECES = {
    "gyoku": ("玉", False),
    "ou": ("王", False),
    "hisha": ("飛", False),
    "kaku": ("角", False),
    "kin": ("金", False),
    "gin": ("銀", False),
    "keima": ("桂", False),
    "kyosha": ("香", False),
    "fu": ("歩", False),
    "ryu": ("龍", True),
    "uma": ("馬", True),
    "narigin": ("全", True),
    "narikei": ("圭", True),
    "narikyo": ("杏", True),
    "tokin": ("と", True),
}


# User-Agent を付けないと Google Fonts は woff2 ではなく TrueType を返す
def load_font(text: str) -> bytes:
    query = urllib.parse.urlencode({"family": f"{FONT_FAMILY}:wght@{FONT_WEIGHT}", "text": text}, safe=":@")
    with urllib.request.urlopen(f"https://fonts.googleapis.com/css2?{query}") as res:
        css = res.read().decode()

    match = re.search(r"src: url\((.+?)\) format\('(?:opentype|truetype)'\)", css)
    if match is None:
        raise RuntimeError(f"Font url not found in css: {css}")

    with urllib.request.urlopen(match.group(1)) as res:
        return res.read()


def render_koma(label: str, promoted: bool, font: skia.Font) -> skia.Image:
    surface = skia.Surface(PIECE_SIZE, PIECE_SIZE)
    canvas = surface.getCanvas()
    canvas.clear(skia.ColorTRANSPARENT)
    canvas.translate(PIECE_SIZE / 2, PIECE_SIZE / 2)

    scale = PIECE_SIZE / 60
    path = skia.Path()
    path.addPoly([skia.Point(x * scale - PIECE_SIZE / 2, y * scale - PIECE_SIZE / 2) for x, y in KOMA_SHAPE], True)

    canvas.drawPath(path, skia.Paint(Color=KOMA, AntiAlias=True))
    canvas.drawPath(
        path,
        skia.Paint(
            Color=KOMA_EDGE,
            Style=skia.Paint.kStroke_Style,
            StrokeWidth=2.5 * scale,
            StrokeJoin=skia.Paint.kRound_Join,
            AntiAlias=True,
        ),
    )

    # ArticleOgImageRenderer.DrawTextCentered と同じく、中心 (0, 3) に文字の上下中央を合わせる
    metrics = font.getMetrics()
    baseline = 3 - (metrics.fAscent + metrics.fDescent) / 2
    width = font.measureText(label)
    paint = skia.Paint(Color=KOMA_PROMOTED if promoted else KOMA_INK, AntiAlias=True)
    canvas.drawString(label, -width / 2, baseline, font, paint)

    return surface.makeImageSnapshot()


def main() -> None:
    labels = "".join(label for label, _ in PIECES.values())
    typeface = skia.Typeface.MakeFromData(skia.Data.MakeWithCopy(load_font(labels)))
    font = skia.Font(typeface, FONT_SIZE)

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, (label, promoted) in PIECES.items():
        render_koma(label, promoted, font).save(str(OUTPUT_DIR / f"{name}.png"), skia.kPNG)
        print(f"generated {name}.png ({label})")


if __name__ == "__main__":
    main()
