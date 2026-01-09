# Components

このディレクトリには、アプリケーション全体で再利用される共通コンポーネントが配置されます。

**重要**: nari-noteではAtomic Designパターンを採用しています。

## ディレクトリ構造

```
components/
├── ui/         # 基本UIコンポーネント（shadcn/ui等）
├── layout/     # レイアウトコンポーネント（ヘッダー、フッターなど）
└── common/     # Atomic Design構造 + ユーティリティコンポーネント
    ├── atoms/       # 最小単位のコンポーネント
    ├── molecules/   # Atomsを組み合わせた機能コンポーネント
    ├── Loading.tsx
    ├── ErrorMessage.tsx
    └── EmptyState.tsx
```

## Atomic Design階層

### Atoms（`common/atoms/`）

最小単位の再利用可能なUIコンポーネント。これ以上分割できない基本要素。

**例:**
- `FormField.tsx` - ラベル + 入力フィールド
- `ErrorAlert.tsx` - エラー表示
- `FormTitle.tsx` - フォームタイトル
- `TagChip.tsx` - タグチップ

**使用例:**
```tsx
import { FormField, ErrorAlert } from '@/components/common/atoms';
```

### Molecules（`common/molecules/`）

複数のAtomsを組み合わせた機能コンポーネント。

**例:**
- `EmailField.tsx` - メールアドレス入力（FormFieldを使用）
- `PasswordField.tsx` - パスワード入力（FormFieldを使用）
- `TagInput.tsx` - タグ入力（Input + Button + TagChipを使用）
- `CharacterCounter.tsx` - 文字数カウンター

**使用例:**
```tsx
import { EmailField, PasswordField } from '@/components/common/molecules';
```

### Organisms（`features/{feature}/organisms/`）

Atoms/Moleculesを組み合わせた完全な機能ブロック。
featuresディレクトリ内で定義されます。

**例:**
- `LoginPage.tsx` - ログインフォーム
- `ArticleFormPage.tsx` - 記事作成フォーム

詳細は [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) の Atomic Design セクションを参照してください。

## UI Components（`ui/`）

基本的なUIコンポーネント。機能に依存しない汎用的なコンポーネント（主にshadcn/ui）。

**例:**
- `Button.tsx` - ボタン
- `Input.tsx` - 入力フィールド
- `Card.tsx` - カード
- `Modal.tsx` - モーダル

**使用例:**
```tsx
import { Button } from '@/components/ui/Button';

<Button variant="primary" onClick={handleClick}>
  クリック
</Button>
```

## Layout Components（`layout/`）

ページのレイアウト構造を提供するコンポーネント。

**例:**
- `Header.tsx` - ヘッダー
- `Footer.tsx` - フッター
- `Sidebar.tsx` - サイドバー

**使用例:**
```tsx
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';

export default function RootLayout({ children }) {
  return (
    <>
      <Header />
      <main>{children}</main>
      <Footer />
    </>
  );
}
```

## Common Components（`common/`）

Atomic Design構造の共通コンポーネントとユーティリティコンポーネント。

詳細は `common/README.md` および `common/ATOMIC_DESIGN.md` を参照してください。

## コンポーネント作成ガイドライン

### 1. Atomic Designを意識する

新しいコンポーネントを作成する前に：

1. **既存のAtomsで対応できないか確認**
2. **既存のMoleculesで対応できないか確認**
3. **適切な粒度（Atoms/Molecules）で分割**

### 2. 再利用性を考慮

- 特定の機能に依存しない
- propsで柔軟にカスタマイズ可能
- 適切なデフォルト値を設定

### 3. 型定義を明確に

```tsx
interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
}

export function Button({ 
  variant = 'primary', 
  size = 'md', 
  children, 
  onClick, 
  disabled = false 
}: ButtonProps) {
  // ...
}
```

### 4. Tailwind CSSでスタイリング

```tsx
const variantClasses = {
  primary: 'bg-blue-500 hover:bg-blue-600 text-white',
  secondary: 'bg-gray-200 hover:bg-gray-300 text-gray-800',
  danger: 'bg-red-500 hover:bg-red-600 text-white',
};
```

## 機能固有のコンポーネント（Organisms）との違い

- **共通コンポーネント（このディレクトリ）**: 複数の機能で再利用される小さな単位（Atoms/Molecules）
- **機能固有コンポーネント（`features/{feature}/organisms/`）**: 特定の機能に特化した完全な機能ブロック

迷った場合は、まず機能固有のOrganismとして作成し、後で共通化（Atoms/Moleculesへの分解）を検討してください。

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) を参照してください。
