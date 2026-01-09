# Features

このディレクトリには、機能ごとにモジュール化されたコンポーネントが配置されます。

**重要**: nari-noteではAtomic Designパターンを採用しています。featuresディレクトリでは主に**Organisms（生体）**として完全な機能ブロックを実装します。

## ディレクトリ構造

各機能（feature）は以下の構造を持ちます：

```
{feature}/
├── organisms/      # Organisms（完全な機能ブロック）
└── types.ts        # 型定義（機能固有の型）
```

**注意**: 以前のcontainers/components/hooks構造から、Atomic Design準拠のorganisms構造に移行しました。

## Atomic Designにおける配置

### Atoms（原子）
`src/components/common/atoms/`
- FormField, ErrorAlert, FormTitle, TagChipなど
- 最小単位の基本要素

### Molecules（分子）
`src/components/common/molecules/`
- EmailField, PasswordField, TagInputなど
- Atomsを組み合わせた機能コンポーネント

### Organisms（生体）
`src/features/{feature}/organisms/`
- LoginPage, ArticleFormPage, UserProfilePageなど
- Atoms/Moleculesを組み合わせた完全な機能ブロック
- データフェッチングやビジネスロジックを含む

## 機能一覧

### article/
記事に関する機能
- **ArticleFormPage.tsx** - 記事作成・編集
- **ArticleDetailPage.tsx** - 記事詳細表示
- **HomeArticleList.tsx** - 記事一覧表示

### auth/
認証に関する機能
- **LoginPage.tsx** - ログイン
- **SignUpPage.tsx** - 新規登録

### user/
ユーザーに関する機能
- **UserProfilePage.tsx** - プロフィール表示・編集

### tag/
タグに関する機能
- **TagArticleListPage.tsx** - タグ別記事一覧

## 新しい機能を追加する場合

1. 機能名のディレクトリを作成（例: `comment/`）
2. `organisms/` ディレクトリを作成
3. Organisms（完全な機能ブロック）を実装
   - まず必要なAtomsが存在するか確認
   - 次に必要なMoleculesが存在するか確認
   - 最後にOrganismsとして完全な機能を実装
4. `types.ts` を作成（機能固有の型定義が必要な場合）

## Organismsの実装パターン

### パターン1: シンプルなフォーム

```tsx
// src/features/auth/organisms/LoginPage.tsx
'use client';

import { useState } from 'react';
import { EmailField, PasswordField } from '@/components/common/molecules';
import { ErrorAlert } from '@/components/common/atoms';
import { Button } from '@/components/ui/button';
import { useLogin } from '@/lib/api';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const login = useLogin();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login.mutate({ email, password });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {login.error && <ErrorAlert message="ログインに失敗しました" />}
      <EmailField value={email} onChange={setEmail} />
      <PasswordField value={password} onChange={setPassword} />
      <Button type="submit" disabled={login.isPending}>
        {login.isPending ? 'ログイン中...' : 'ログイン'}
      </Button>
    </form>
  );
}
```

### パターン2: データ取得を伴うOrganism（Container/Presentational分離）

データフェッチングが複雑な場合は、Container/Presentationalパターンを併用できます。

**Container:**
```tsx
// src/features/article/organisms/ArticleDetailContainer.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetailPage } from './ArticleDetailPage';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleDetailContainerProps {
  articleId: number;
}

export function ArticleDetailContainer({ articleId }: ArticleDetailContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="エラー" onRetry={refetch} />;
  if (!data) return null;

  return <ArticleDetailPage article={data} />;
}
```

**Presentational Organism:**
```tsx
// src/features/article/organisms/ArticleDetailPage.tsx
import type { GetArticleResponse } from '@/lib/api/types';
import { TagChip } from '@/components/common/atoms';

interface ArticleDetailPageProps {
  article: GetArticleResponse;
}

export function ArticleDetailPage({ article }: ArticleDetailPageProps) {
  return (
    <article className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold">{article.title}</h1>
      <div className="prose">{article.body}</div>
      <div className="flex gap-2">
        {article.tags?.map((tag, index) => (
          <TagChip key={index} tag={tag} />
        ))}
      </div>
    </article>
  );
}
```

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) を参照してください。
