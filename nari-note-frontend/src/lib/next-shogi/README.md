# next-shogi

BOD形式の将棋盤面をパースして、React コンポーネントとしてレンダリングする軽量ライブラリ。

## 特徴

- 🪶 **軽量**: 外部依存なし（React と Tailwind CSS のみ）
- 🎨 **カスタマイズ可能**: Tailwind CSS でスタイリング
- 📱 **レスポンシブ**: 3段階のサイズ調整（sm/md/lg）
- 🔒 **型安全**: TypeScript strict mode 対応
- ⚡ **高速**: useMemo によるパース結果のメモ化

## インストール

プロジェクト内のライブラリなので、インストールは不要です。

## 使い方

### 基本的な使い方

```tsx
import { ShogiBoard } from '@/lib/next-shogi';

const bodText = `
後手の持駒：なし
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---+---+---+---+---+---+---+---+---+
|v香|v桂|v銀|v金|v玉|v金|v銀|v桂|v香|一
+---+---+---+---+---+---+---+---+---+
| ・|v飛| ・| ・| ・| ・| ・|v角| ・|二
+---+---+---+---+---+---+---+---+---+
|v歩|v歩|v歩|v歩|v歩|v歩|v歩|v歩|v歩|三
+---+---+---+---+---+---+---+---+---+
| ・| ・| ・| ・| ・| ・| ・| ・| ・|四
+---+---+---+---+---+---+---+---+---+
| ・| ・| ・| ・| ・| ・| ・| ・| ・|五
+---+---+---+---+---+---+---+---+---+
| ・| ・| ・| ・| ・| ・| ・| ・| ・|六
+---+---+---+---+---+---+---+---+---+
| 歩| 歩| 歩| 歩| 歩| 歩| 歩| 歩| 歩|七
+---+---+---+---+---+---+---+---+---+
| ・| 角| ・| ・| ・| ・| ・| 飛| ・|八
+---+---+---+---+---+---+---+---+---+
| 香| 桂| 銀| 金| 玉| 金| 銀| 桂| 香|九
+---+---+---+---+---+---+---+---+---+
先手の持駒：なし
`;

function App() {
  return <ShogiBoard bodText={bodText} />;
}
```

### オプション

```tsx
<ShogiBoard
  bodText={bodText}
  size="md" // 'sm' | 'md' | 'lg'
  showCapturedPieces={true} // 持駒を表示
  showPlayerNames={true} // プレイヤー名を表示
  className="my-custom-class"
/>
```

### 低レベル API

個々のコンポーネントを使用することもできます。

```tsx
import { parseBOD, Board, CapturedPieces } from '@/lib/next-shogi';

function CustomBoard() {
  const parsed = parseBOD(bodText);

  return (
    <div>
      <CapturedPieces
        pieces={parsed.captured.gote}
        owner="gote"
        size="md"
      />
      <Board board={parsed.board} size="md" />
      <CapturedPieces
        pieces={parsed.captured.sente}
        owner="sente"
        size="md"
      />
    </div>
  );
}
```

## BOD形式について

BOD（Board Object Diagram）形式は、将棋の盤面を表現するテキスト形式です。

### 基本構造

```
後手の持駒：[持駒リスト]
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---+---+---+---+---+---+---+---+---+
|[駒]|[駒]|[駒]|[駒]|[駒]|[駒]|[駒]|[駒]|[駒]|一
+---+---+---+---+---+---+---+---+---+
...（9行）
+---+---+---+---+---+---+---+---+---+
先手の持駒：[持駒リスト]
先手：[プレイヤー名]（オプション）
後手：[プレイヤー名]（オプション）
```

### 駒の表記

- 空マス: `・`
- 先手の駒: `玉`、`飛`、`角`、`金`、`銀`、`桂`、`香`、`歩`
- 後手の駒: `v` 接頭辞（例: `v玉`、`v飛`）
- 成駒: `龍`、`馬`、`成銀`、`成桂`、`成香`、`と`

### 持駒の表記

- なし: `なし`
- あり: `飛　金三　銀二`（駒種 + 枚数、枚数は漢数字、1枚の場合は省略可）

## API リファレンス

### コンポーネント

#### `<ShogiBoard>`

メインコンポーネント。BOD形式のテキストから将棋盤面を表示します。

**Props:**

- `bodText: string` - BOD形式のテキスト（必須）
- `size?: 'sm' | 'md' | 'lg'` - サイズ（デフォルト: `'md'`）
- `showCapturedPieces?: boolean` - 持駒を表示（デフォルト: `true`）
- `showPlayerNames?: boolean` - プレイヤー名を表示（デフォルト: `true`）
- `className?: string` - 追加のクラス名

#### `<Board>`

9x9の盤面コンポーネント。

**Props:**

- `board: BoardState` - 盤面状態（必須）
- `size?: 'sm' | 'md' | 'lg'` - サイズ（デフォルト: `'md'`）
- `className?: string` - 追加のクラス名

#### `<Piece>`

駒1つを表示するコンポーネント。

**Props:**

- `piece: Piece` - 駒情報（必須）
- `size?: 'sm' | 'md' | 'lg'` - サイズ（デフォルト: `'md'`）

#### `<CapturedPieces>`

持駒を表示するコンポーネント。

**Props:**

- `pieces: CapturedPiece[]` - 持駒リスト（必須）
- `owner: 'sente' | 'gote'` - 所有者（必須）
- `playerName?: string` - プレイヤー名
- `size?: 'sm' | 'md' | 'lg'` - サイズ（デフォルト: `'md'`）

### 関数

#### `parseBOD(bodText: string): ParsedBoard`

BOD形式のテキストをパースして、盤面データを返します。

**戻り値:**

```typescript
interface ParsedBoard {
  board: BoardState; // 9x9の盤面
  captured: CapturedPieces; // 持駒
  sentePlayer?: string; // 先手のプレイヤー名
  gotePlayer?: string; // 後手のプレイヤー名
}
```

#### `parseCapturedPieces(text: string): CapturedPiece[]`

持駒文字列をパースして、持駒リストを返します。

## スタイリング

このライブラリは Tailwind CSS を使用しています。以下のカラーパレットが使用されています：

- 盤面背景: `bg-amber-50`
- マス線: `border-amber-700`
- 盤面外枠: `border-amber-900`
- 駒・テキスト: `text-amber-900`

カスタムスタイルを適用する場合は、`className` プロップを使用してください。

## 制限事項

- BOD形式のパースは基本的なフォーマットのみサポートしています
- 指し手の表示やアニメーションは未実装です
- 盤面の編集機能はありません

## ライセンス

プロジェクト内部ライブラリのため、プロジェクトと同じライセンスが適用されます。
