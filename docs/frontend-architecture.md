# フロントエンドアーキテクチャガイド

このドキュメントは、nari-note-frontendの実装時に参照するアーキテクチャの詳細ガイドです。

## 概要

nari-note-frontendは、**Container/Presentationalパターン**と**Atomic Designパターン**を組み合わせたNext.jsアプリケーションです。

- **Container/Presentationalパターン**: ビジネスロジック（データ取得、状態管理）とUI表示を明確に分離
- **Atomic Designパターン**: UIコンポーネントを小さな単位（Atoms → Molecules → Organisms）で階層的に構成

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
│   │   │   ├── organisms/            # Organisms（完全な機能ブロック）
│   │   │   │   ├── ArticleFormPage.tsx      # 記事作成・編集フォーム
│   │   │   │   ├── ArticleDetailPage.tsx    # 記事詳細表示
│   │   │   │   └── HomeArticleList.tsx      # 記事一覧表示
│   │   │   └── types.ts              # 記事機能の型定義
│   │   ├── auth/                     # 認証機能
│   │   │   ├── organisms/
│   │   │   │   ├── LoginPage.tsx            # ログインフォーム
│   │   │   │   └── SignUpPage.tsx           # 新規登録フォーム
│   │   │   └── types.ts
│   │   ├── user/                     # ユーザー機能
│   │   │   ├── organisms/
│   │   │   │   └── UserProfilePage.tsx      # ユーザープロフィール
│   │   │   └── types.ts
│   │   └── tag/                      # タグ機能
│   │       └── organisms/
│   │           └── TagArticleListPage.tsx   # タグ別記事一覧
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
│   │   └── common/                   # Atomic Design構造の共通コンポーネント
│   │       ├── atoms/                # 最小単位のコンポーネント
│   │       │   ├── FormField.tsx     # ラベル + 入力フィールド
│   │       │   ├── ErrorAlert.tsx    # エラー表示
│   │       │   ├── FormTitle.tsx     # フォームタイトル
│   │       │   └── TagChip.tsx       # タグチップ
│   │       ├── molecules/            # Atomsを組み合わせたコンポーネント
│   │       │   ├── EmailField.tsx    # メール入力フィールド
│   │       │   ├── PasswordField.tsx # パスワード入力フィールド
│   │       │   ├── NameField.tsx     # 名前入力フィールド
│   │       │   ├── TagInput.tsx      # タグ入力
│   │       │   └── CharacterCounter.tsx # 文字数カウンター
│   │       ├── Loading.tsx           # ローディング表示
│   │       ├── ErrorMessage.tsx      # エラーメッセージ
│   │       └── EmptyState.tsx        # 空状態表示
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

**注意**: Atomic Designパターンの導入により、featuresディレクトリ内では主に**Organisms（生体）**として完全な機能ブロックを実装します。細かいコンポーネントはAtomsやMoleculesとして`components/common/`に配置します。

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

## Atomic Designパターン

### 概要

Atomic Designは、UIコンポーネントを化学の原子・分子・生体の概念で階層的に構成する設計手法です。小さく再利用可能なコンポーネントを組み合わせて、複雑なUIを構築します。

### Atoms（原子）- 最小単位のコンポーネント

**配置:** `src/components/common/atoms/`

**特徴:**
- これ以上分割できない最小単位の基本要素
- shadcn UIコンポーネントまたは基本的なHTML要素で構築
- 他のコンポーネントに依存しない
- 単一責任の原則に従う

**例:**
- **FormField.tsx** - ラベル + 入力フィールドのセット
- **ErrorAlert.tsx** - エラーメッセージ表示
- **FormTitle.tsx** - フォームタイトル
- **TagChip.tsx** - タグチップ（削除ボタン付き）

**実装例:**
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
  placeholder?: string;
  error?: string;
}

export function FormField({
  id,
  label,
  type = 'text',
  value,
  onChange,
  placeholder,
  error,
}: FormFieldProps) {
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

### Molecules（分子）- Atomsを組み合わせたコンポーネント

**配置:** `src/components/common/molecules/`

**特徴:**
- 複数のAtomsを組み合わせて特定の機能を実装
- 再利用可能な機能単位
- 独自のビジネスロジックは持たない
- propsで柔軟にカスタマイズ可能

**例:**
- **EmailField.tsx** - メールアドレス入力（FormFieldを使用）
- **PasswordField.tsx** - パスワード入力（FormFieldを使用）
- **NameField.tsx** - ユーザー名入力（FormFieldを使用）
- **TagInput.tsx** - タグ入力（Input + Button + TagChipを使用）
- **CharacterCounter.tsx** - 文字数カウンター

**実装例:**
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
      placeholder="example@example.com"
      error={error}
    />
  );
}
```

### Organisms（生体）- 完全な機能を持つコンポーネント

**配置:** `src/features/{feature}/organisms/`

**特徴:**
- Atoms/Moleculesを組み合わせた完全な機能ブロック
- ビジネスロジックやデータフェッチングを含む
- Container/Presentationalパターンと組み合わせて使用可能
- 特定の機能に特化

**例:**
- **LoginPage.tsx** - ログインフォーム（EmailField + PasswordField + Buttonを使用）
- **SignUpPage.tsx** - 新規登録フォーム（NameField + EmailField + PasswordFieldを使用）
- **ArticleFormPage.tsx** - 記事作成・編集フォーム（複数のMoleculesを組み合わせ）

**実装例:**
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
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});
  
  const login = useLogin({
    onSuccess: () => {
      // ログイン成功時の処理
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login.mutate({ email, password });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <EmailField 
        value={email} 
        onChange={setEmail} 
        error={errors.email}
      />
      <PasswordField 
        value={password} 
        onChange={setPassword}
        error={errors.password}
      />
      <Button type="submit" disabled={login.isPending}>
        {login.isPending ? 'ログイン中...' : 'ログイン'}
      </Button>
    </form>
  );
}
```

### インポート方法

**Atomsのインポート:**
```tsx
import { FormField, ErrorAlert, FormTitle, TagChip } from '@/components/common/atoms';
```

**Moleculesのインポート:**
```tsx
import { EmailField, PasswordField, TagInput } from '@/components/common/molecules';
```

**Organismsのインポート:**
```tsx
import { LoginPage } from '@/features/auth/organisms/LoginPage';
```

### Atomic Designのメリット

1. **再利用性**: 小さなコンポーネントを組み合わせて複雑なUIを構築
2. **保守性**: 各コンポーネントが独立しているため、変更の影響範囲が明確
3. **テスト容易性**: 小さな単位でテストが可能
4. **一貫性**: 同じコンポーネントを使用することでUIの一貫性を保つ
5. **レビュー容易性**: ファイルが細かく分かれているため、レビューがしやすい
6. **開発効率**: AIエージェントが小さな粒度で実装できる

### 実装ガイドライン

1. **Atoms**: 最小単位で、他のコンポーネントに依存しない
2. **Molecules**: Atomsを組み合わせて特定の機能を実装
3. **Organisms**: featuresディレクトリ内でAtoms/Moleculesを組み合わせて完全な機能を実装
4. **各コンポーネントは単一責任の原則に従う**
5. **propsの型定義を明確にする**
6. **新しいコンポーネントを作る前に、既存のAtoms/Moleculesで対応できないか確認する**

## コンポーネント分類

### 1. Atoms（原子）- 最小単位

**配置:** `src/components/common/atoms/`

基本的なUI要素。これ以上分割できない最小単位。

**例:**
- `FormField.tsx` - フォームフィールド
- `ErrorAlert.tsx` - エラーアラート
- `FormTitle.tsx` - フォームタイトル
- `TagChip.tsx` - タグチップ

### 2. Molecules（分子）- 機能単位

**配置:** `src/components/common/molecules/`

Atomsを組み合わせた機能コンポーネント。

**例:**
- `EmailField.tsx` - メールアドレス入力
- `PasswordField.tsx` - パスワード入力
- `TagInput.tsx` - タグ入力
- `CharacterCounter.tsx` - 文字数カウンター

### 3. Organisms（生体）- 完全な機能ブロック

**配置:** `src/features/{feature}/organisms/`

特定の機能に紐づく完全なコンポーネント。Atoms/Moleculesを組み合わせて実装。

**例:**
- `LoginPage.tsx` - ログインフォーム
- `ArticleFormPage.tsx` - 記事作成・編集フォーム
- `UserProfilePage.tsx` - ユーザープロフィール

### 4. UI Components（基本UIコンポーネント）

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

### 4. Common Components（その他共通コンポーネント）

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

1. **Atomic Designパターン**を理解する
   - Atoms: 最小単位のコンポーネント
   - Molecules: Atomsを組み合わせた機能コンポーネント
   - Organisms: 完全な機能ブロック
2. **Container/Presentationalパターン**を理解する（必要に応じて適用）
3. **components/common/atoms, molecules**に共通コンポーネントを配置
4. **features/{feature}/organisms**に機能固有のOrganismsを配置
5. **TanStack Query フック**でデータフェッチング
6. **型定義**を活用して型安全に開発

### AIエージェントが生成すべきコード

1. **Atoms**: `src/components/common/atoms/`
2. **Molecules**: `src/components/common/molecules/`
3. **Organisms**: `src/features/{feature}/organisms/`
4. **Page Component**: `src/app/{route}/page.tsx`

### 実装の順序

1. まず必要なAtomsが存在するか確認・作成
2. 次にAtomsを組み合わせてMoleculesを作成
3. 最後にMoleculesを組み合わせてOrganismsを実装
4. PageコンポーネントからOrganismsを呼び出す

## 関連ドキュメント

- [API使用方法](./frontend-api-usage.md)
- [認証戦略](./authentication-strategy.md)
- [アーキテクチャ概要](./architecture-overview.md)
