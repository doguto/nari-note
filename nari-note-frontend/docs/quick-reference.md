# フロントエンドクイックリファレンス

開発者とAIエージェントのための簡易リファレンスです。

**重要**: nari-noteではAtomic Designパターンを採用しています。小さな単位（Atoms → Molecules → Organisms）でコンポーネントを構築してください。

## ディレクトリ配置

| 何を作る？ | どこに配置？ | ファイル名 |
|-----------|------------|-----------|
| 最小単位のコンポーネント | `src/components/common/atoms/` | `FormField.tsx`, `ErrorAlert.tsx` |
| 機能コンポーネント | `src/components/common/molecules/` | `EmailField.tsx`, `TagInput.tsx` |
| 完全な機能ブロック | `src/features/{feature}/organisms/` | `LoginPage.tsx`, `ArticleFormPage.tsx` |
| ボタンコンポーネント | `src/components/ui/` | `Button.tsx` |
| ヘッダーコンポーネント | `src/components/layout/` | `Header.tsx` |
| ローディング表示 | `src/components/common/` | `Loading.tsx` |
| 記事詳細ページ | `src/app/articles/[id]/` | `page.tsx` |
| 日付フォーマット関数 | `src/lib/utils/` | `format.ts` |
| カスタムフック（共通） | `src/lib/hooks/` | `useDebounce.ts` |

## Atomic Design階層

### Atoms（原子）- 最小単位
```
components/common/atoms/
├── FormField.tsx       # ラベル + 入力フィールド
├── ErrorAlert.tsx      # エラー表示
├── FormTitle.tsx       # フォームタイトル
└── TagChip.tsx         # タグチップ
```

### Molecules（分子）- 機能単位
```
components/common/molecules/
├── EmailField.tsx          # メール入力（FormFieldを使用）
├── PasswordField.tsx       # パスワード入力
├── NameField.tsx           # 名前入力
├── TagInput.tsx            # タグ入力
└── CharacterCounter.tsx    # 文字数カウンター
```

### Organisms（生体）- 完全な機能
```
features/{feature}/organisms/
├── LoginPage.tsx           # ログインフォーム
├── SignUpPage.tsx          # 新規登録フォーム
├── ArticleFormPage.tsx     # 記事作成フォーム
└── ArticleDetailPage.tsx   # 記事詳細表示
```

## コンポーネント作成テンプレート

### Atom（最小単位）

```tsx
// src/components/common/atoms/FormField.tsx
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface FormFieldProps {
  id: string;
  label: string;
  type?: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
}

export function FormField({ id, label, type = 'text', value, onChange, error }: FormFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>{label}</Label>
      <Input
        id={id}
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}
```

### Molecule（機能単位）

```tsx
// src/components/common/molecules/EmailField.tsx
import { FormField } from '@/components/common/atoms/FormField';

interface EmailFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
}

export function EmailField({ value, onChange, error }: EmailFieldProps) {
  return (
    <FormField
      id="email"
      label="メールアドレス"
      type="email"
      value={value}
      onChange={onChange}
      error={error}
    />
  );
}
```

### Organism（完全な機能）

```tsx
// src/features/auth/organisms/LoginPage.tsx
'use client';

import { useState } from 'react';
import { EmailField } from '@/components/common/molecules/EmailField';
import { PasswordField } from '@/components/common/molecules/PasswordField';
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
      <EmailField value={email} onChange={setEmail} />
      <PasswordField value={password} onChange={setPassword} />
      <Button type="submit" disabled={login.isPending}>
        {login.isPending ? 'ログイン中...' : 'ログイン'}
      </Button>
    </form>
  );
}
```

### Organism + Container（データ取得を伴う場合）

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

```tsx
// src/features/article/organisms/ArticleDetailPage.tsx
import type { GetArticleResponse } from '@/lib/api/types';

interface ArticleDetailPageProps {
  article: GetArticleResponse;
}

export function ArticleDetailPage({ article }: ArticleDetailPageProps) {
  return (
    <article className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold">{article.title}</h1>
      <div className="prose">{article.body}</div>
    </article>
  );
}
```

### Page Component

```tsx
// src/app/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailContainer } from '@/features/article/organisms/ArticleDetailContainer';

export default function ArticleDetailPage() {
  const params = useParams();
  const articleId = Number(params.id);

  return <ArticleDetailContainer articleId={articleId} />;
}
```

### Custom Hook (必要に応じて)

```tsx
import { useState } from 'react';
import { useCreateArticle } from '@/lib/api';

export function useArticleForm() {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  
  const createArticle = useCreateArticle({
    onSuccess: () => {
      // 成功時の処理
    },
  });

  const handleSubmit = () => {
    createArticle.mutate({ title, body, tags: [] });
  };

  return {
    title,
    setTitle,
    body,
    setBody,
    handleSubmit,
    isSubmitting: createArticle.isPending,
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

// Atoms
import { FormField, ErrorAlert, FormTitle, TagChip } from '@/components/common/atoms';

// Molecules
import { EmailField, PasswordField, TagInput, CharacterCounter } from '@/components/common/molecules';

// 共通コンポーネント
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { EmptyState } from '@/components/common/EmptyState';

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

### Atomic Design
- [ ] 既存のAtomsで対応できないか確認したか
- [ ] 既存のMoleculesで対応できないか確認したか
- [ ] 適切な粒度（Atoms/Molecules/Organisms）で分割されているか

### 基本事項
- [ ] 型定義が明確か
- [ ] `'use client'`が必要な場所（データフェッチングを含むOrganism）に記述されているか
- [ ] Loading/ErrorMessageを使用しているか
- [ ] ブランドカラーを使用しているか
- [ ] 適切なディレクトリに配置されているか

## 詳細ドキュメント

- [フロントエンドアーキテクチャガイド](/docs/architecture.md) - 完全なアーキテクチャ説明
- [フロントエンド実装ガイド](/docs/implementation-guide.md) - 詳細な実装パターン
- [API使用方法](/docs/api-usage.md) - API使用の詳細
