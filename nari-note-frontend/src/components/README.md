# Components

このディレクトリには、アプリケーション全体で再利用される共通コンポーネントが配置されます。

**重要**: nari-noteでは5層のAtomic Designパターン（Page → Template → Organism → Molecule → Atom）を採用しています。

## ディレクトリ構造

```
components/
├── ui/                # Atom層（汎用的な最小単位のUIコンポーネント）
└── molecules/         # Molecule層（汎用的な複合コンポーネント）
```

## 5層のAtomic Design階層

### Atom（`ui/`）

汎用的な最小単位のUIコンポーネント。これ以上分割できない基本要素。
Shadcn等のUIコンポーネントもここに配置。

**例:**
- `button.tsx` - ボタン
- `input.tsx` - 入力フィールド
- `label.tsx` - ラベル
- `FormField.tsx` - ラベル + 入力フィールド
- `ErrorAlert.tsx` - エラー表示
- `TagChip.tsx` - タグチップ

**使用例:**
```tsx
import { Button, Input, FormField, ErrorAlert } from '@/components/ui';
```

### Molecule（`molecules/`）

複数のAtomsを組み合わせた汎用的な複合コンポーネント。

**例:**
- `ArticleCard.tsx` - 記事カード
- `UserIcon.tsx` - ユーザーアイコン
- `EmailField.tsx` - メールアドレス入力
- `PasswordField.tsx` - パスワード入力
- `TagInput.tsx` - タグ入力

**使用例:**
```tsx
import { ArticleCard, EmailField, PasswordField } from '@/components/molecules';
```

### Organism（`features/{feature}/organisms/`）

各Template特有のUI単位。featuresディレクトリ内で定義されます。

**例:**
- `TitleForm.tsx` - タイトル入力フォーム
- `BodyForm.tsx` - 本文入力フォーム

### Template（`features/{feature}/templates/`）

各ページのUI構成に責任を持つ。featuresディレクトリ内で定義されます。

**例:**
- `ArticleFormTemplate.tsx` - 記事フォームのUI構成

### Page（`features/{feature}/pages/`）

ページのロジックに責任を持つ（UIなし）。featuresディレクトリ内で定義されます。

**例:**
- `ArticleEditPage.tsx` - 記事編集ロジック

詳細は [フロントエンドアーキテクチャガイド](/docs/architecture.md) を参照してください。

## コンポーネント作成ガイドライン

### 1. 5層のAtomic Designを意識する

新しいコンポーネントを作成する前に：

1. **既存のAtomsで対応できないか確認**（`components/ui/`）
2. **既存のMoleculesで対応できないか確認**（`components/molecules/`）
3. **適切な層（Atom/Molecule/Organism/Template/Page）で分割**

### 2. 配置場所の判断基準

- **Atom**: 汎用的な最小単位（極力サイズ等は上位レイヤーで調整）→ `components/ui/`
- **Molecule**: 汎用的な複合コンポーネント（極力サイズ等は上位レイヤーで調整）→ `components/molecules/`
- **Organism**: Template特有のUI単位 → `features/{feature}/organisms/`
- **Template**: UI構成とレスポンシブ対応 → `features/{feature}/templates/`
- **Page**: ロジックのみ（UIなし）→ `features/{feature}/pages/`

### 3. 再利用性を考慮

- 特定の機能に依存しない
- propsで柔軟にカスタマイズ可能
- 適切なデフォルト値を設定

### 4. 型定義を明確に

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

### 5. Tailwind CSSでスタイリング

```tsx
const variantClasses = {
  primary: 'bg-blue-500 hover:bg-blue-600 text-white',
  secondary: 'bg-gray-200 hover:bg-gray-300 text-gray-800',
  danger: 'bg-red-500 hover:bg-red-600 text-white',
};
```

## 機能固有のコンポーネント（Organism以上）との違い

- **共通コンポーネント（このディレクトリ）**: 複数の機能で再利用される小さな単位（Atom/Molecule）
- **機能固有コンポーネント（`features/{feature}/`）**: 特定の機能に特化したOrganism/Template/Page

迷った場合は、まず機能固有のOrganismとして作成し、後で共通化（Atom/Moleculeへの分解）を検討してください。

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/architecture.md) を参照してください。
