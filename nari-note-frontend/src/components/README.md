# Components

このディレクトリには、アプリケーション全体で再利用される共通コンポーネントが配置されます。

## ディレクトリ構造

```
components/
├── layout/     # レイアウトコンポーネント（ヘッダー、フッターなど）
└── common/     # その他共通コンポーネント（ローディング、エラー表示など）
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

アプリケーション全体で使用される共通のユーティリティコンポーネント。

**例:**
- `Loading.tsx` - ローディング表示
- `ErrorMessage.tsx` - エラーメッセージ
- `EmptyState.tsx` - 空状態表示

**使用例:**
```tsx
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

if (isLoading) return <Loading />;
if (error) return <ErrorMessage message="エラーが発生しました" />;
```

## コンポーネント作成ガイドライン

### 1. 再利用性を考慮

- 特定の機能に依存しない
- propsで柔軟にカスタマイズ可能
- 適切なデフォルト値を設定

### 2. 型定義を明確に

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

### 3. Tailwind CSSでスタイリング

```tsx
const variantClasses = {
  primary: 'bg-blue-500 hover:bg-blue-600 text-white',
  secondary: 'bg-gray-200 hover:bg-gray-300 text-gray-800',
  danger: 'bg-red-500 hover:bg-red-600 text-white',
};
```

## 機能固有のコンポーネントとの違い

- **共通コンポーネント（このディレクトリ）**: 複数の機能で再利用される
- **機能固有コンポーネント（`features/{feature}/components/`）**: 特定の機能でのみ使用される

迷った場合は、まず機能固有のコンポーネントとして作成し、後で共通化を検討してください。

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) を参照してください。
