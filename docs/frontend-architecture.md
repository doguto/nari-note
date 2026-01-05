# フロントエンドアーキテクチャガイド

このドキュメントは、nari-note-frontendの実装時に参照するアーキテクチャの詳細ガイドです。

## 概要

nari-note-frontendは、**Container/Presentationalパターン**を採用したNext.jsアプリケーションです。
このパターンにより、ビジネスロジック（データ取得、状態管理）とUI表示を明確に分離します。

## 技術スタック

### フレームワーク・ライブラリ
- **Next.js 15**: React フレームワーク（App Router）
- **React 19**: UIライブラリ
- **TypeScript**: 型安全な開発

### データフェッチング・状態管理
- **TanStack Query (React Query)**: サーバーステート管理
- **Axios**: HTTP クライアント

### スタイリング
- **Tailwind CSS 4**: ユーティリティファーストCSSフレームワーク
- **shadcn/ui**: 再利用可能なUIコンポーネント（今後追加予定）

### エディタ・マークダウン
- **react-markdown**: マークダウンレンダリング（今後追加予定）
- **@uiw/react-md-editor**: マークダウンエディタ（今後追加予定）

## ディレクトリ構造

```
nari-note-frontend/
├── src/
│   ├── app/                          # Next.js App Router
│   │   ├── (auth)/                   # 認証関連ページグループ
│   │   │   ├── login/
│   │   │   │   └── page.tsx
│   │   │   └── signup/
│   │   │       └── page.tsx
│   │   ├── articles/                 # 記事関連ページ
│   │   │   ├── [id]/
│   │   │   │   ├── page.tsx          # 記事詳細ページ
│   │   │   │   └── edit/
│   │   │   │       └── page.tsx      # 記事編集ページ
│   │   │   └── new/
│   │   │       └── page.tsx          # 記事作成ページ
│   │   ├── users/                    # ユーザー関連ページ
│   │   │   └── [id]/
│   │   │       └── page.tsx          # ユーザープロフィールページ
│   │   ├── layout.tsx                # ルートレイアウト
│   │   ├── page.tsx                  # トップページ
│   │   └── globals.css               # グローバルスタイル
│   ├── features/                     # 機能ごとのモジュール
│   │   ├── article/                  # 記事機能
│   │   │   ├── components/           # 記事機能専用コンポーネント
│   │   │   │   ├── ArticleCard.tsx           # Presentational
│   │   │   │   ├── ArticleDetail.tsx         # Presentational
│   │   │   │   ├── ArticleForm.tsx           # Presentational
│   │   │   │   └── ArticleList.tsx           # Presentational
│   │   │   ├── containers/           # 記事機能のContainer
│   │   │   │   ├── ArticleCardContainer.tsx
│   │   │   │   ├── ArticleDetailContainer.tsx
│   │   │   │   ├── ArticleFormContainer.tsx
│   │   │   │   └── ArticleListContainer.tsx
│   │   │   ├── hooks/                # 記事機能専用カスタムフック
│   │   │   │   └── useArticleForm.ts
│   │   │   └── types.ts              # 記事機能の型定義
│   │   ├── auth/                     # 認証機能
│   │   │   ├── components/
│   │   │   │   ├── LoginForm.tsx             # Presentational
│   │   │   │   └── SignUpForm.tsx            # Presentational
│   │   │   ├── containers/
│   │   │   │   ├── LoginFormContainer.tsx
│   │   │   │   └── SignUpFormContainer.tsx
│   │   │   ├── hooks/
│   │   │   │   └── useAuth.ts
│   │   │   └── types.ts
│   │   └── user/                     # ユーザー機能
│   │       ├── components/
│   │       │   ├── UserProfile.tsx           # Presentational
│   │       │   └── UserProfileForm.tsx       # Presentational
│   │       ├── containers/
│   │       │   ├── UserProfileContainer.tsx
│   │       │   └── UserProfileFormContainer.tsx
│   │       ├── hooks/
│   │       │   └── useUserProfile.ts
│   │       └── types.ts
│   ├── components/                   # 共通UIコンポーネント
│   │   ├── ui/                       # 基本UIコンポーネント（shadcn/ui等）
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Card.tsx
│   │   │   └── Modal.tsx
│   │   ├── layout/                   # レイアウトコンポーネント
│   │   │   ├── Header.tsx
│   │   │   ├── Footer.tsx
│   │   │   └── Sidebar.tsx
│   │   └── common/                   # その他共通コンポーネント
│   │       ├── Loading.tsx
│   │       ├── ErrorMessage.tsx
│   │       └── EmptyState.tsx
│   ├── lib/                          # 共通ロジック・ユーティリティ
│   │   ├── api/                      # API関連
│   │   │   ├── client.ts             # Axiosクライアント設定
│   │   │   ├── endpoints.ts          # APIエンドポイント定義
│   │   │   ├── hooks.ts              # TanStack Query フック
│   │   │   ├── types.ts              # API型定義
│   │   │   └── index.ts              # エクスポート
│   │   ├── providers/                # Reactプロバイダー
│   │   │   ├── QueryProvider.tsx     # TanStack Query プロバイダー
│   │   │   └── AuthProvider.tsx      # 認証プロバイダー（今後追加）
│   │   ├── utils/                    # ユーティリティ関数
│   │   │   ├── format.ts             # フォーマット関数
│   │   │   ├── validation.ts         # バリデーション関数
│   │   │   └── date.ts               # 日付操作関数
│   │   ├── hooks/                    # 共通カスタムフック
│   │   │   ├── useDebounce.ts
│   │   │   └── useLocalStorage.ts
│   │   └── constants/                # 定数定義
│   │       └── index.ts
│   └── types/                        # グローバル型定義
│       └── index.ts
├── public/                           # 静的ファイル
│   ├── images/
│   └── favicon.ico
├── .env.local.example                # 環境変数サンプル
├── next.config.ts                    # Next.js設定
├── tailwind.config.ts                # Tailwind CSS設定
├── tsconfig.json                     # TypeScript設定
└── package.json                      # パッケージ定義
```

## Container/Presentationalパターン

### 概要

このパターンは、コンポーネントを**Container（データ管理）**と**Presentational（表示）**の2つの役割に分離します。

### Container Component（コンテナコンポーネント）

**責務:**
- データフェッチング（TanStack Query フックの使用）
- ビジネスロジックの実行
- 状態管理
- イベントハンドラの実装
- Presentational Componentへのprops渡し

**配置場所:** `src/features/{feature}/containers/`

**命名規則:** `{ComponentName}Container.tsx`

**例:**
```tsx
// src/features/article/containers/ArticleDetailContainer.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetail } from '../components/ArticleDetail';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleDetailContainerProps {
  articleId: number;
}

export function ArticleDetailContainer({ articleId }: ArticleDetailContainerProps) {
  const { data, isLoading, error } = useGetArticle({ id: articleId });

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return <ErrorMessage message="記事の取得に失敗しました" />;
  }

  if (!data) {
    return <ErrorMessage message="記事が見つかりません" />;
  }

  return <ArticleDetail article={data} />;
}
```

### Presentational Component（表示コンポーネント）

**責務:**
- UIの表示のみ
- propsを受け取って表示
- DOMイベントをpropsのコールバック関数に渡す
- スタイリング

**配置場所:** `src/features/{feature}/components/` または `src/components/`

**命名規則:** `{ComponentName}.tsx`

**特徴:**
- `'use client'` ディレクティブは不要（親がClient Componentなら不要）
- APIフックを使用しない
- 状態を持たない（または表示のみの状態のみ）

**例:**
```tsx
// src/features/article/components/ArticleDetail.tsx
import { GetArticleResponse } from '@/lib/api/types';

interface ArticleDetailProps {
  article: GetArticleResponse;
}

export function ArticleDetail({ article }: ArticleDetailProps) {
  return (
    <article className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-4">{article.title}</h1>
      <div className="flex items-center gap-4 mb-6">
        <span className="text-gray-600">著者: {article.authorName}</span>
        <span className="text-gray-600">いいね: {article.likeCount}</span>
      </div>
      <div className="prose max-w-none">
        {article.body}
      </div>
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2 mt-6">
          {article.tags.map((tag, index) => (
            <span
              key={index}
              className="px-3 py-1 bg-gray-100 rounded-full text-sm"
            >
              {tag}
            </span>
          ))}
        </div>
      )}
    </article>
  );
}
```

## コンポーネント分類

### 1. Feature Components（機能コンポーネント）

**配置:** `src/features/{feature}/components/`

特定の機能に紐づくコンポーネント。他の機能では再利用しない。

**例:**
- `ArticleCard.tsx` - 記事カード
- `ArticleList.tsx` - 記事一覧
- `LoginForm.tsx` - ログインフォーム

### 2. UI Components（基本UIコンポーネント）

**配置:** `src/components/ui/`

アプリケーション全体で再利用される基本的なUIコンポーネント。
機能に依存しない汎用的なコンポーネント。

**例:**
- `Button.tsx` - ボタン
- `Input.tsx` - 入力フィールド
- `Card.tsx` - カード
- `Modal.tsx` - モーダル

### 3. Layout Components（レイアウトコンポーネント）

**配置:** `src/components/layout/`

ページのレイアウト構造を提供するコンポーネント。

**例:**
- `Header.tsx` - ヘッダー
- `Footer.tsx` - フッター
- `Sidebar.tsx` - サイドバー

### 4. Common Components（共通コンポーネント）

**配置:** `src/components/common/`

アプリケーション全体で使用される共通のコンポーネント。

**例:**
- `Loading.tsx` - ローディング表示
- `ErrorMessage.tsx` - エラーメッセージ
- `EmptyState.tsx` - 空状態表示

## ファイル命名規則

### コンポーネントファイル

- **PascalCase** を使用
- コンポーネント名とファイル名を一致させる
- 拡張子は `.tsx`

**例:**
```
ArticleCard.tsx
ArticleDetailContainer.tsx
Button.tsx
```

### 非コンポーネントファイル

- **camelCase** を使用
- 用途が明確な名前をつける
- 拡張子は `.ts` または `.tsx`

**例:**
```
useArticleForm.ts
format.ts
validation.ts
types.ts
```

### ディレクトリ

- **小文字** + **ハイフン** を使用（Next.js App Routerの規約に従う）
- または **camelCase** を使用（features, components配下）

**例:**
```
article/
auth/
user/
ui/
```

## API使用パターン

### 1. TanStack Query フックを使用（推奨）

Container Componentで使用します。

**データ取得:**
```tsx
import { useGetArticle } from '@/lib/api';

const { data, isLoading, error } = useGetArticle({ id: articleId });
```

**データ作成・更新:**
```tsx
import { useCreateArticle } from '@/lib/api';

const createArticle = useCreateArticle({
  onSuccess: (data) => {
    console.log('記事作成成功', data);
  },
  onError: (error) => {
    console.error('記事作成エラー', error);
  },
});

createArticle.mutate({
  title: '新しい記事',
  body: '内容',
  tags: ['TypeScript'],
});
```

### 2. 直接API関数を呼び出す

Server Componentや特殊なケースで使用します。

```tsx
import { articlesApi } from '@/lib/api';

const article = await articlesApi.getArticle({ id: 1 });
```

## カスタムフックの作成

複雑なロジックや複数のAPIフックを組み合わせる場合は、カスタムフックを作成します。

**配置:** `src/features/{feature}/hooks/`

**例:**
```tsx
// src/features/article/hooks/useArticleForm.ts
import { useState } from 'react';
import { useCreateArticle, useUpdateArticle } from '@/lib/api';
import type { CreateArticleRequest } from '@/lib/api/types';

interface UseArticleFormOptions {
  articleId?: number;
  onSuccess?: () => void;
}

export function useArticleForm({ articleId, onSuccess }: UseArticleFormOptions = {}) {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);

  const isEditMode = !!articleId;

  const createArticle = useCreateArticle({
    onSuccess: () => {
      resetForm();
      onSuccess?.();
    },
  });

  const updateArticle = useUpdateArticle({
    onSuccess: () => {
      onSuccess?.();
    },
  });

  const handleSubmit = () => {
    const data: CreateArticleRequest = {
      title,
      body,
      tags,
    };

    if (isEditMode) {
      updateArticle.mutate({ id: articleId, ...data });
    } else {
      createArticle.mutate(data);
    }
  };

  const resetForm = () => {
    setTitle('');
    setBody('');
    setTags([]);
  };

  return {
    title,
    setTitle,
    body,
    setBody,
    tags,
    setTags,
    handleSubmit,
    isSubmitting: createArticle.isPending || updateArticle.isPending,
    isEditMode,
  };
}
```

## ページの実装パターン

### 1. Server Component（デフォルト）

データフェッチングが不要、または初期データのみ必要な場合。

```tsx
// src/app/page.tsx
import { ArticleListContainer } from '@/features/article/containers/ArticleListContainer';

export default function HomePage() {
  return (
    <div>
      <h1>記事一覧</h1>
      <ArticleListContainer />
    </div>
  );
}
```

### 2. Client Component

インタラクティブな機能が必要な場合。

```tsx
// src/app/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailContainer } from '@/features/article/containers/ArticleDetailContainer';

export default function ArticleDetailPage() {
  const params = useParams();
  const articleId = Number(params.id);

  return <ArticleDetailContainer articleId={articleId} />;
}
```

## スタイリング規約

### Tailwind CSS の使用

- ユーティリティクラスを使用
- カスタムCSSは最小限に
- レスポンシブデザインは `sm:`, `md:`, `lg:` プレフィックスを使用

**例:**
```tsx
<div className="max-w-4xl mx-auto p-6">
  <h1 className="text-3xl font-bold mb-4">タイトル</h1>
  <p className="text-gray-600">テキスト</p>
</div>
```

### カラーパレット

nari-noteのブランドカラー:
- プライマリ: `#88b04b` (緑)
- セカンダリ: `#2d3e1f` (ダークグリーン)
- 背景: `#f5f3e8` (ベージュ)

Tailwind configで定義して使用します。

## 型定義

### APIレスポンス型

`src/lib/api/types.ts` で定義されています。

```tsx
import type { GetArticleResponse } from '@/lib/api/types';
```

### コンポーネントProps型

インターフェースで定義します。

```tsx
interface ArticleCardProps {
  article: GetArticleResponse;
  onLike?: () => void;
}

export function ArticleCard({ article, onLike }: ArticleCardProps) {
  // ...
}
```

### Feature固有の型

`src/features/{feature}/types.ts` で定義します。

```tsx
// src/features/article/types.ts
export interface ArticleFormState {
  title: string;
  body: string;
  tags: string[];
}
```

## エラーハンドリング

### APIエラー

TanStack Queryの `error` を使用します。

```tsx
const { data, error } = useGetArticle({ id: articleId });

if (error) {
  return <ErrorMessage message="記事の取得に失敗しました" />;
}
```

### フォームバリデーション

Container Componentで実装します。

```tsx
const [errors, setErrors] = useState<{ title?: string; body?: string }>({});

const validate = () => {
  const newErrors: typeof errors = {};
  
  if (!title) {
    newErrors.title = 'タイトルは必須です';
  }
  
  if (!body) {
    newErrors.body = '本文は必須です';
  }
  
  setErrors(newErrors);
  return Object.keys(newErrors).length === 0;
};
```

## パフォーマンス最適化

### 1. React.memo

Presentational Componentで使用します。

```tsx
import { memo } from 'react';

export const ArticleCard = memo(function ArticleCard({ article }: ArticleCardProps) {
  // ...
});
```

### 2. useCallback

Container Componentのコールバック関数で使用します。

```tsx
const handleLike = useCallback(() => {
  toggleLike.mutate({ articleId });
}, [articleId, toggleLike]);
```

### 3. 動的インポート

大きなコンポーネントは動的にインポートします。

```tsx
import dynamic from 'next/dynamic';

const MarkdownEditor = dynamic(() => import('@/components/ui/MarkdownEditor'), {
  loading: () => <Loading />,
  ssr: false,
});
```

## テスト（今後追加予定）

- **Unit Test**: 個別コンポーネントのテスト
- **Integration Test**: Container/Presentationalの統合テスト
- **E2E Test**: ユーザーシナリオのテスト

## まとめ

### 開発者が覚えるべきこと

1. **Container/Presentationalパターン**を理解する
2. **features配下**に機能ごとのコンポーネントを配置
3. **components配下**に共通コンポーネントを配置
4. **TanStack Query フック**をContainerで使用
5. **型定義**を活用して型安全に開発

### AIエージェントが生成すべきコード

1. **Container Component**: `src/features/{feature}/containers/`
2. **Presentational Component**: `src/features/{feature}/components/`
3. **Custom Hook**: `src/features/{feature}/hooks/`
4. **Page Component**: `src/app/{route}/page.tsx`

## 関連ドキュメント

- [API使用方法](./frontend-api-usage.md)
- [認証戦略](./authentication-strategy.md)
- [アーキテクチャ概要](./architecture-overview.md)
