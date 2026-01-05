# UI Components

基本的なUIコンポーネントを配置するディレクトリです。

## 概要

このディレクトリには、shadcn/uiベースのコンポーネントを配置しています。
アプリケーション全体で再利用される基本的なUIコンポーネントで、機能に依存しない汎用的なコンポーネントです。

## 導入されているコンポーネント

### Button
ボタンコンポーネント

**バリアント:**
- `default`: 標準のプライマリボタン
- `secondary`: セカンダリボタン
- `destructive`: 削除などの破壊的アクションに使用
- `outline`: アウトラインボタン
- `ghost`: 背景なしボタン
- `link`: リンクスタイルのボタン

**サイズ:**
- `sm`: 小サイズ
- `default`: 標準サイズ
- `lg`: 大サイズ
- `icon`: アイコン専用（正方形）

```tsx
import { Button } from '@/components/ui';

<Button variant="default" size="default">
  送信
</Button>
```

### Input
入力フィールドコンポーネント

```tsx
import { Input } from '@/components/ui';

<Input 
  type="email"
  placeholder="example@example.com"
  value={email}
  onChange={(e) => setEmail(e.target.value)}
/>
```

### Textarea
複数行テキスト入力コンポーネント

```tsx
import { Textarea } from '@/components/ui';

<Textarea
  placeholder="メッセージを入力"
  value={message}
  onChange={(e) => setMessage(e.target.value)}
  rows={5}
/>
```

### Label
フォームラベルコンポーネント

```tsx
import { Label } from '@/components/ui';

<Label htmlFor="email">メールアドレス</Label>
<Input id="email" type="email" />
```

### Card
カードコンポーネント（複数のサブコンポーネントで構成）

**サブコンポーネント:**
- `Card`: カードコンテナ
- `CardHeader`: ヘッダー部分
- `CardTitle`: タイトル
- `CardDescription`: 説明文
- `CardContent`: メインコンテンツ
- `CardFooter`: フッター部分

```tsx
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/components/ui';

<Card>
  <CardHeader>
    <CardTitle>カードタイトル</CardTitle>
    <CardDescription>説明文がここに入ります</CardDescription>
  </CardHeader>
  <CardContent>
    <p>カードのコンテンツ</p>
  </CardContent>
  <CardFooter>
    <Button>アクション</Button>
  </CardFooter>
</Card>
```

## インポート方法

すべてのコンポーネントは `@/components/ui` からインポートできます：

```tsx
import { Button, Input, Label, Card, CardContent } from '@/components/ui';
```

個別のファイルからもインポート可能です：

```tsx
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
```

## デモページ

すべてのコンポーネントの使用例は `/ui-demo` ページで確認できます。

## 技術スタック

- **shadcn/ui**: コンポーネントのベース
- **Radix UI**: アクセシブルなプリミティブ
- **Tailwind CSS**: スタイリング
- **class-variance-authority**: バリアント管理
- **clsx & tailwind-merge**: クラス名の結合と最適化

## カスタマイズ

コンポーネントのスタイルは `src/app/globals.css` のCSS変数で調整できます：

```css
:root {
  --primary: 0 0% 9%;
  --secondary: 0 0% 96%;
  --destructive: 0 84% 60%;
  /* その他の変数 */
}
```

## 作成ガイドライン

1. **Presentationalコンポーネント**として作成
2. **propsで柔軟にカスタマイズ**可能にする
3. **Tailwind CSS**でスタイリング
4. **型定義**を明確にする
5. **デフォルト値**を適切に設定
6. **アクセシビリティ**を考慮

## 新しいコンポーネントの追加

shadcn/uiから追加のコンポーネントを導入する場合：

```bash
npx shadcn@latest add [component-name]
```

ただし、Tailwind CSS v4を使用しているため、生成されたコンポーネントのクラス名を調整する必要がある場合があります：
- `bg-primary` → `bg-[hsl(var(--primary))]`
- `text-muted-foreground` → `text-[hsl(var(--muted-foreground))]`

