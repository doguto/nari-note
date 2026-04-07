# フロントエンドクイックリファレンス

開発者とAIエージェントのための簡易リファレンスです。

**重要**: nari-noteではAtomic Designパターン（5層構造）を採用しています。階層的に（Atom → Molecule → Organism → Template → Page）コンポーネントを構築してください。

## ディレクトリ配置

| 何を作る？ | どこに配置？ | ファイル名 |
|-----------|------------|-----------|
| 最小単位のUIコンポーネント | `src/components/ui/` | `Button.tsx`, `Input.tsx`, `Card.tsx` |
| 汎用複合コンポーネント | `src/components/molecules/` | `FormField.tsx`, `SearchBar.tsx` |
| 機能固有のUI単位 | `src/features/{feature}/organisms/` | `LoginForm.tsx`, `ArticleCard.tsx` |
| UI構成とレイアウト | `src/features/{feature}/templates/` | `LoginTemplate.tsx`, `ArticleDetailTemplate.tsx` |
| ロジック層（UIなし） | `src/features/{feature}/pages/` | `LoginPage.tsx`, `ArticleDetailPage.tsx` |
| グローバルコンポーネント | `src/features/global/organisms/` | `Header.tsx`, `Footer.tsx` |
| Next.jsページファイル | `src/app/{route}/` | `page.tsx` |
| 日付フォーマット関数 | `src/lib/utils/` | `format.ts` |
| カスタムフック（共通） | `src/lib/hooks/` | `useDebounce.ts` |

**features配下の機能モジュール**: `auth`, `article`, `global`, `tag`, `user`

## Atomic Design階層（5層構造）

### Atoms（原子）- 汎用的な最小単位のUIコンポーネント
```
components/ui/
├── button.tsx          # ボタン（shadcn/ui）
├── input.tsx           # インプット
├── card.tsx            # カード
├── label.tsx           # ラベル
└── badge.tsx           # バッジ
```

### Molecules（分子）- 汎用的な複合コンポーネント
```
components/molecules/
├── FormField.tsx       # ラベル + インプット + エラー表示
├── SearchBar.tsx       # 検索バー
├── TagChip.tsx         # タグチップ
└── AlertMessage.tsx    # アラートメッセージ
```

### Organisms（生体）- Template特有のUI単位
```
features/{feature}/organisms/
├── LoginForm.tsx           # ログインフォーム（auth）
├── ArticleCard.tsx         # 記事カード（article）
├── TagList.tsx             # タグリスト（tag）
├── UserProfile.tsx         # ユーザープロフィール（user）
└── Header.tsx              # ヘッダー（global）
```

### Templates（テンプレート）- UI構成とレスポンシブ対応
```
features/{feature}/templates/
├── LoginTemplate.tsx           # ログインページUI（auth）
├── ArticleDetailTemplate.tsx   # 記事詳細ページUI（article）
├── ArticleListTemplate.tsx     # 記事一覧ページUI（article）
└── UserProfileTemplate.tsx     # ユーザープロフィールページUI（user）
```

### Pages（ページ）- ロジックのみ、UIなし
```
features/{feature}/pages/
├── LoginPage.tsx               # ログインページロジック（auth）
├── ArticleDetailPage.tsx       # 記事詳細ページロジック（article）
├── ArticleCreatePage.tsx       # 記事作成ページロジック（article）
└── UserProfilePage.tsx         # ユーザープロフィールページロジック（user）
```

## コンポーネント作成テンプレート

### Atom（汎用的な最小単位のUIコンポーネント）

```tsx
// src/components/ui/button.tsx
import * as React from "react";

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'default' | 'outline' | 'ghost';
  size?: 'default' | 'sm' | 'lg';
}

export function Button({ variant = 'default', size = 'default', className, ...props }: ButtonProps) {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-md ${className}`}
      {...props}
    />
  );
}
```

### Molecule（汎用的な複合コンポーネント）

```tsx
// src/components/molecules/FormField.tsx
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface FormFieldProps {
  id: string;
  label: string;
  type?: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  placeholder?: string;
}

export function FormField({ id, label, type = 'text', value, onChange, error, placeholder }: FormFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>{label}</Label>
      <Input
        id={id}
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}
```

### Organism（Template特有のUI単位）

```tsx
// src/features/auth/organisms/LoginForm.tsx
import { FormField } from '@/components/molecules/FormField';
import { Button } from '@/components/ui/button';

interface LoginFormProps {
  email: string;
  password: string;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isSubmitting: boolean;
  error?: string;
}

export function LoginForm({
  email,
  password,
  onEmailChange,
  onPasswordChange,
  onSubmit,
  isSubmitting,
  error
}: LoginFormProps) {
  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <FormField
        id="email"
        label="メールアドレス"
        type="email"
        value={email}
        onChange={onEmailChange}
      />
      <FormField
        id="password"
        label="パスワード"
        type="password"
        value={password}
        onChange={onPasswordChange}
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'ログイン中...' : 'ログイン'}
      </Button>
    </form>
  );
}
```

### Template（UI構成とレスポンシブ対応）

```tsx
// src/features/auth/templates/LoginTemplate.tsx
import { LoginForm } from '@/features/auth/organisms/LoginForm';

interface LoginTemplateProps {
  email: string;
  password: string;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isSubmitting: boolean;
  error?: string;
}

export function LoginTemplate(props: LoginTemplateProps) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-[#f5f3e8]">
      <div className="w-full max-w-md p-8 bg-white rounded-lg shadow-lg">
        <h1 className="text-2xl font-bold text-center mb-6">ログイン</h1>
        <LoginForm {...props} />
      </div>
    </div>
  );
}
```

### Page（ロジックのみ、UIなし）

```tsx
// src/features/auth/pages/LoginPage.tsx
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useLogin } from '@/lib/api';
import { LoginTemplate } from '@/features/auth/templates/LoginTemplate';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const router = useRouter();
  
  const login = useLogin({
    onSuccess: () => {
      router.push('/');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login.mutate({ email, password });
  };

  return (
    <LoginTemplate
      email={email}
      password={password}
      onEmailChange={setEmail}
      onPasswordChange={setPassword}
      onSubmit={handleSubmit}
      isSubmitting={login.isPending}
      error={login.error?.message}
    />
  );
}
```

### Next.js App Router Page

```tsx
// src/app/login/page.tsx
import { LoginPage } from '@/features/auth/pages/LoginPage';

export default function Page() {
  return <LoginPage />;
}
```

### データ取得を伴うPage

```tsx
// src/features/article/pages/ArticleDetailPage.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetailTemplate } from '@/features/article/templates/ArticleDetailTemplate';
import { Loading } from '@/components/molecules/Loading';
import { ErrorMessage } from '@/components/molecules/ErrorMessage';

interface ArticleDetailPageProps {
  articleId: number;
}

export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="記事の取得に失敗しました" onRetry={refetch} />;
  if (!data) return null;

  return <ArticleDetailTemplate article={data} />;
}
```

### Custom Hook（Page層で使用するロジックフック）

```tsx
// src/features/article/hooks/useArticleForm.ts
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useCreateArticle } from '@/lib/api';

export function useArticleForm() {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const router = useRouter();
  
  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      router.push(`/articles/${data.id}`);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createArticle.mutate({ title, body, tags });
  };

  return {
    title,
    setTitle,
    body,
    setBody,
    tags,
    setTags,
    handleSubmit,
    isSubmitting: createArticle.isPending,
    error: createArticle.error,
  };
}
```

## API使用

### データ取得（GET）

```tsx
import { useGetArticle } from '@/lib/api';

const { data, isLoading, error, refetch } = useGetArticle({ id: 1 });
```

### データ作成・更新（POST/PUT）

```tsx
import { useCreateArticle } from '@/lib/api';

const createArticle = useCreateArticle({
  onSuccess: (data) => {
    console.log('成功', data);
  },
  onError: (error) => {
    console.error('エラー', error);
  },
});

createArticle.mutate({
  title: 'タイトル',
  body: '本文',
  tags: ['TypeScript'],
});
```

## よく使うインポート

```tsx
// API
import { useGetArticle, useCreateArticle } from '@/lib/api';
import type { GetArticleResponse } from '@/lib/api/types';

// Atoms (components/ui/)
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';

// Molecules (components/molecules/)
import { FormField } from '@/components/molecules/FormField';
import { SearchBar } from '@/components/molecules/SearchBar';
import { TagChip } from '@/components/molecules/TagChip';
import { Loading } from '@/components/molecules/Loading';
import { ErrorMessage } from '@/components/molecules/ErrorMessage';

// Organisms (features/{feature}/organisms/)
import { LoginForm } from '@/features/auth/organisms/LoginForm';
import { ArticleCard } from '@/features/article/organisms/ArticleCard';
import { Header } from '@/features/global/organisms/Header';

// Templates (features/{feature}/templates/)
import { LoginTemplate } from '@/features/auth/templates/LoginTemplate';
import { ArticleDetailTemplate } from '@/features/article/templates/ArticleDetailTemplate';

// Pages (features/{feature}/pages/)
import { LoginPage } from '@/features/auth/pages/LoginPage';
import { ArticleDetailPage } from '@/features/article/pages/ArticleDetailPage';

// Next.js
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
```

## スタイリング（Tailwind CSS）

### nari-noteブランドカラー

```tsx
// プライマリ（緑）
className="bg-[#88b04b] text-white"

// セカンダリ（ダークグリーン）
className="bg-[#2d3e1f] text-white"

// 背景（ベージュ）
className="bg-[#f5f3e8]"

// ボーダー
className="border border-[#d4cdb3]"
```

### よく使うクラス

```tsx
// コンテナ
className="max-w-7xl mx-auto px-4"

// カード
className="bg-white rounded-lg shadow p-6"

// ボタン（プライマリ）
className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939]"

// 入力フィールド
className="w-full px-4 py-2 border rounded"

// グリッドレイアウト
className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
```

## チェックリスト

新しいコンポーネントを作成したら：

### Atomic Design（5層構造）
- [ ] 既存のAtoms（components/ui/）で対応できないか確認したか
- [ ] 既存のMolecules（components/molecules/）で対応できないか確認したか
- [ ] Organismは特定のTemplate専用のUIになっているか
- [ ] Templateは適切なレスポンシブ対応を含んでいるか
- [ ] Pageはロジックのみに専念しているか（UIを含んでいないか）
- [ ] 適切な粒度（Atom → Molecule → Organism → Template → Page）で分割されているか

### 基本事項
- [ ] 型定義が明確か
- [ ] `'use client'`が必要な場所（Page層）に記述されているか
- [ ] Loading/ErrorMessageコンポーネントを使用しているか
- [ ] ブランドカラーを使用しているか
- [ ] 適切なディレクトリに配置されているか
  - Atoms: `components/ui/`
  - Molecules: `components/molecules/`
  - Organisms: `features/{feature}/organisms/`
  - Templates: `features/{feature}/templates/`
  - Pages: `features/{feature}/pages/`

## 詳細ドキュメント

- [フロントエンドアーキテクチャガイド](/docs/architecture.md) - 完全なアーキテクチャ説明
- [フロントエンド実装ガイド](/docs/implementation-guide.md) - 詳細な実装パターン
- [API使用方法](/docs/api-usage.md) - API使用の詳細
