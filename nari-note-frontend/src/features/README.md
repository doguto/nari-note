# Features

このディレクトリには、機能ごとにモジュール化されたコンポーネントが配置されます。

**重要**: nari-noteでは5層のAtomic Designパターン（Page → Template → Organism → Molecule → Atom）を採用しています。

## ディレクトリ構造

各機能（feature）は以下の3層構造を持ちます：

```
{feature}/
├── pages/          # Page層（ロジックのみ、UIなし）
├── templates/      # Template層（UI構成とレスポンシブ対応）
├── organisms/      # Organism層（Template特有のUI単位）
└── types.ts        # 型定義（機能固有の型）
```

## 5層のAtomic Designにおける配置

### Atom（原子）
`src/components/ui/`
- Button, Input, Labelなど
- 汎用的な最小単位のUIコンポーネント
- Shadcn等のUIコンポーネントもここに配置

### Molecule（分子）
`src/components/molecules/`
- ArticleCard, UserIconなど
- 汎用的な複合コンポーネント
- Atomが組み合わさって構成される

### Organism（生体）
`src/features/{feature}/organisms/`
- TitleForm, BodyFormなど
- 各Template特有のUI単位
- Atoms/Moleculesを組み合わせて実装

### Template（テンプレート）
`src/features/{feature}/templates/`
- ArticleFormTemplateなど
- 各ページのUI構成に責任を持つ
- レスポンシブ対応とレイアウト切り替えを担当

### Page（ページ）
`src/features/{feature}/pages/`
- ArticleEditPageなど
- ページのロジックに責任を持つ（UIなし）
- バックエンドとの通信等の非UIロジックを持つ

## 機能一覧

### article/
記事に関する機能
- pages/ - 記事機能のロジック
- templates/ - 記事ページのUI構成
- organisms/ - 記事フォームなどのUI単位

### auth/
認証に関する機能
- pages/ - 認証ロジック
- templates/ - 認証ページのUI構成
- organisms/ - ログインフォームなどのUI単位

### user/
ユーザーに関する機能
- pages/ - ユーザー機能のロジック
- templates/ - ユーザーページのUI構成
- organisms/ - プロフィール関連のUI単位

### tag/
タグに関する機能
- pages/ - タグ機能のロジック
- templates/ - タグページのUI構成
- organisms/ - タグ表示などのUI単位

### global/
グローバル共通コンポーネント
- organisms/ - Header, Footer, Sidebarなど

## 新しい機能を追加する場合

1. 機能名のディレクトリを作成（例: `comment/`）
2. `pages/`, `templates/`, `organisms/` ディレクトリを作成
3. 5層構造に従って実装
   - まず必要なAtomsが存在するか確認（`components/ui/`）
   - 次に必要なMoleculesが存在するか確認（`components/molecules/`）
   - Organismsとして特有のUI単位を実装
   - TemplatesでUI構成を実装
   - PagesでロジックとTemplateの連携を実装
4. `types.ts` を作成（機能固有の型定義が必要な場合）

## 実装パターン

### パターン1: Page（ロジック層）

```tsx
// src/features/auth/pages/LoginPage.tsx
'use client';

import { useState } from 'react';
import { useLogin } from '@/lib/api';
import { LoginTemplate } from '../templates/LoginTemplate';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const login = useLogin();

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
      isLoading={login.isPending}
      error={login.error}
    />
  );
}
```

### パターン2: Template（UI構成層）

```tsx
// src/features/auth/templates/LoginTemplate.tsx
import { EmailField, PasswordField } from '@/components/molecules';
import { ErrorAlert } from '@/components/ui';
import { Button } from '@/components/ui/button';

interface LoginTemplateProps {
  email: string;
  password: string;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isLoading: boolean;
  error?: Error;
}

export function LoginTemplate({
  email,
  password,
  onEmailChange,
  onPasswordChange,
  onSubmit,
  isLoading,
  error,
}: LoginTemplateProps) {
  return (
    <div className="max-w-md mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">ログイン</h1>
      {error && <ErrorAlert message="ログインに失敗しました" />}
      <form onSubmit={onSubmit} className="space-y-4">
        <EmailField value={email} onChange={onEmailChange} />
        <PasswordField value={password} onChange={onPasswordChange} />
        <Button type="submit" disabled={isLoading} className="w-full">
          {isLoading ? 'ログイン中...' : 'ログイン'}
        </Button>
      </form>
    </div>
  );
}
```

### パターン3: データ取得を伴うPage

```tsx
// src/features/article/pages/ArticleDetailPage.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';

interface ArticleDetailPageProps {
  articleId: number;
}

export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message="エラー" onRetry={refetch} />;
  if (!data) return null;

  return <ArticleDetailTemplate article={data} />;
}
```

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/architecture.md) を参照してください。
