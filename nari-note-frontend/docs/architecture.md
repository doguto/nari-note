# フロントエンドアーキテクチャガイド

このドキュメントは、nari-note-frontendの実装時に参照するアーキテクチャの詳細ガイドです。

## 概要

nari-note-frontendは、**Atomic Designパターン（5層構造）**を採用したNext.jsアプリケーションです。

- **5層構造**: Page（ロジック層）→ Template（UI構成層）→ Organism（特化UI層）→ Molecule（汎用複合層）→ Atom（汎用最小層）

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
│   │   │   ├── pages/                # Page層（ロジック）
│   │   │   │   ├── ArticleFormPage.tsx      # 記事作成・編集ページ
│   │   │   │   ├── ArticleDetailPage.tsx    # 記事詳細ページ
│   │   │   │   └── HomeArticleListPage.tsx  # 記事一覧ページ
│   │   │   ├── templates/            # Template層（UI構成）
│   │   │   │   ├── ArticleFormTemplate.tsx
│   │   │   │   ├── ArticleDetailTemplate.tsx
│   │   │   │   └── HomeArticleListTemplate.tsx
│   │   │   ├── organisms/            # Organism層（Template特有のUI）
│   │   │   │   ├── ArticleForm.tsx
│   │   │   │   ├── ArticleDetail.tsx
│   │   │   │   └── ArticleList.tsx
│   │   │   └── types.ts              # 記事機能の型定義
│   │   ├── auth/                     # 認証機能
│   │   │   ├── pages/
│   │   │   │   ├── LoginPage.tsx
│   │   │   │   └── SignUpPage.tsx
│   │   │   ├── templates/
│   │   │   │   ├── LoginTemplate.tsx
│   │   │   │   └── SignUpTemplate.tsx
│   │   │   ├── organisms/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   └── SignUpForm.tsx
│   │   │   └── types.ts
│   │   ├── user/                     # ユーザー機能
│   │   │   ├── pages/
│   │   │   │   └── UserProfilePage.tsx
│   │   │   ├── templates/
│   │   │   │   └── UserProfileTemplate.tsx
│   │   │   ├── organisms/
│   │   │   │   └── UserProfile.tsx
│   │   │   └── types.ts
│   │   ├── tag/                      # タグ機能
│   │   │   ├── pages/
│   │   │   │   └── TagArticleListPage.tsx
│   │   │   ├── templates/
│   │   │   │   └── TagArticleListTemplate.tsx
│   │   │   └── organisms/
│   │   │       └── TagArticleList.tsx
│   │   └── global/                   # グローバル機能
│   │       └── organisms/
│   │           ├── Header.tsx
│   │           ├── Footer.tsx
│   │           └── Sidebar.tsx
│   ├── components/                   # 共通UIコンポーネント
│   │   ├── ui/                       # Atom層（汎用最小単位、shadcn/ui等）
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── FormField.tsx         # ラベル + 入力フィールド
│   │   │   ├── ErrorAlert.tsx        # エラー表示
│   │   │   ├── FormTitle.tsx         # フォームタイトル
│   │   │   └── TagChip.tsx           # タグチップ
│   │   ├── molecules/                # Molecule層（汎用複合コンポーネント）
│   │   │   ├── EmailField.tsx        # メール入力フィールド
│   │   │   ├── PasswordField.tsx     # パスワード入力フィールド
│   │   │   ├── NameField.tsx         # 名前入力フィールド
│   │   │   ├── TagInput.tsx          # タグ入力
│   │   │   └── CharacterCounter.tsx  # 文字数カウンター
│   │   ├── Loading.tsx               # ローディング表示
│   │   ├── ErrorMessage.tsx          # エラーメッセージ
│   │   └── EmptyState.tsx            # 空状態表示
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

## 5層構造のAtomic Designパターン

### 概要

この設計では、UIコンポーネントを5つの層（Page、Template、Organism、Molecule、Atom）で階層的に構成します。各層は明確な責務を持ち、上位層は下位層に依存しますが、逆方向の依存はありません。

```
Page（ロジック層）
    ↓ 依存
Template（UI構成層）
    ↓ 依存
Organism（特化UI層）
    ↓ 依存
Molecule（汎用複合層）
    ↓ 依存
Atom（汎用最小層）
```

### 1. Page層（ロジック層）

**配置:** `src/features/{feature}/pages/`

**責務:**
- ページのロジックに責任を持つ
- UIには一切責任を持たない
- バックエンドとの通信等の非UIロジックを持つ
- データフェッチング（TanStack Query フックの使用）
- ビジネスロジックの実行
- 状態管理
- イベントハンドラの実装
- Templateへのprops渡し

**命名規則:** `{ComponentName}Page.tsx`

**例:**
```tsx
// src/features/article/pages/ArticleDetailPage.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { Loading } from '@/components/Loading';
import { ErrorMessage } from '@/components/ErrorMessage';

interface ArticleDetailPageProps {
  articleId: number;
}

export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
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

  return <ArticleDetailTemplate article={data} />;
}
```

### 2. Template層（UI構成層）

**配置:** `src/features/{feature}/templates/`

**責務:**
- 各ページのUI構成に責任を持つ
- レスポンシブ対応によりレイアウトが大きく変わる場合の切り替えを担当
- 他のレスポンシブデザインも基本的にこのレイヤーで担当
- OrganismsやMoleculesを配置・レイアウト
- スタイリング（Tailwind CSS）

**命名規則:** `{ComponentName}Template.tsx`

**特徴:**
- `'use client'` ディレクティブは不要（親がClient Componentなら不要）
- APIフックを使用しない
- 表示のみの状態は持っても良い（例: タブの開閉状態）

**例:**
```tsx
// src/features/article/templates/ArticleDetailTemplate.tsx
import { GetArticleResponse } from '@/lib/api/types';
import { ArticleDetail } from '../organisms/ArticleDetail';
import { Container } from '@/components/ui/Container';

interface ArticleDetailTemplateProps {
  article: GetArticleResponse;
}

export function ArticleDetailTemplate({ article }: ArticleDetailTemplateProps) {
  return (
    <Container className="py-8">
      <div className="max-w-4xl mx-auto">
        {/* デスクトップレイアウト */}
        <div className="hidden md:block">
          <ArticleDetail article={article} layout="wide" />
        </div>
        
        {/* モバイルレイアウト */}
        <div className="block md:hidden">
          <ArticleDetail article={article} layout="compact" />
        </div>
      </div>
    </Container>
  );
}
```

### 3. Organism層（特化UI層）

**配置:** `src/features/{feature}/organisms/`

**責務:**
- 各Template特有のUI単位
- MoleculesやAtomsを組み合わせた機能ブロック
- 特定の機能に特化したコンポーネント
- どうしてもこのレイヤーで調整しないといけないもののみ特例でレスポンシブデザインの調整を行っても良い

**命名規則:** `{ComponentName}.tsx`

**特徴:**
- propsで柔軟にカスタマイズ可能
- 機能固有のロジック（表示ロジックのみ）を持つことがある
- データフェッチングは行わない（Pageから受け取る）

**例:**
```tsx
// src/features/article/organisms/ArticleDetail.tsx
import { GetArticleResponse } from '@/lib/api/types';
import { TagChip } from '@/components/ui/TagChip';
import { Card } from '@/components/ui/Card';

interface ArticleDetailProps {
  article: GetArticleResponse;
  layout?: 'wide' | 'compact';
}

export function ArticleDetail({ article, layout = 'wide' }: ArticleDetailProps) {
  const isWide = layout === 'wide';
  
  return (
    <Card className={isWide ? 'p-8' : 'p-4'}>
      <h1 className={isWide ? 'text-4xl' : 'text-2xl'}>{article.title}</h1>
      <div className="flex items-center gap-4 my-4">
        <span className="text-gray-600">著者: {article.authorName}</span>
        <span className="text-gray-600">いいね: {article.likeCount}</span>
      </div>
      <div className="prose max-w-none">
        {article.body}
      </div>
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2 mt-6">
          {article.tags.map((tag, index) => (
            <TagChip key={index} tag={tag} />
          ))}
        </div>
      )}
    </Card>
  );
}
```

### 4. Molecule層（汎用複合層）

**配置:** `src/components/molecules/`

**責務:**
- 汎用的な複合コンポーネント
- Atomが組み合わさって構成される
- 複数のAtomsを組み合わせて特定の機能を実装
- 独自のビジネスロジックは持たない
- 極力サイズ等は上位レイヤーのCSSで調整出来るように

**命名規則:** `{ComponentName}.tsx`

**例:**
- **EmailField.tsx** - メールアドレス入力（FormFieldを使用）
- **PasswordField.tsx** - パスワード入力（FormFieldを使用）
- **NameField.tsx** - ユーザー名入力（FormFieldを使用）
- **TagInput.tsx** - タグ入力（Input + Button + TagChipを使用）
- **CharacterCounter.tsx** - 文字数カウンター

**実装例:**
```tsx
// src/components/molecules/EmailField.tsx
import { FormField } from '@/components/ui/FormField';

interface EmailFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  className?: string;
}

export function EmailField({ value, onChange, error, className }: EmailFieldProps) {
  return (
    <FormField
      id="email"
      label="メールアドレス"
      type="email"
      value={value}
      onChange={onChange}
      placeholder="example@example.com"
      error={error}
      className={className}
    />
  );
}
```

### 5. Atom層（汎用最小層）

**配置:** `src/components/ui/`

**責務:**
- 汎用的な最小単位のUIコンポーネント
- Shadcn等のUIコンポーネントもここに置く
- これ以上分割できない最小単位の基本要素
- 他のコンポーネントに依存しない
- 単一責任の原則に従う
- 極力サイズ等は上位レイヤーのCSSで調整出来るように

**命名規則:** `{ComponentName}.tsx`

**例:**
- **Button.tsx** - ボタン（shadcn/ui）
- **Input.tsx** - 入力フィールド（shadcn/ui）
- **Card.tsx** - カード（shadcn/ui）
- **FormField.tsx** - ラベル + 入力フィールドのセット
- **ErrorAlert.tsx** - エラーメッセージ表示
- **FormTitle.tsx** - フォームタイトル
- **TagChip.tsx** - タグチップ（削除ボタン付き）

**実装例:**
```tsx
// src/components/ui/FormField.tsx
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
  className?: string;
}

export function FormField({
  id,
  label,
  type = 'text',
  value,
  onChange,
  placeholder,
  error,
  className,
}: FormFieldProps) {
  return (
    <div className={className}>
      <Label htmlFor={id}>{label}</Label>
      <Input
        id={id}
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="mt-2"
      />
      {error && <p className="text-sm text-red-500 mt-1">{error}</p>}
    </div>
  );
}
```

### インポート方法

**Atomsのインポート:**
```tsx
import { FormField, ErrorAlert, FormTitle, TagChip } from '@/components/ui';
```

**Moleculesのインポート:**
```tsx
import { EmailField, PasswordField, TagInput } from '@/components/molecules';
```

**Organismsのインポート:**
```tsx
import { ArticleDetail } from '@/features/article/organisms/ArticleDetail';
```

**Templatesのインポート:**
```tsx
import { ArticleDetailTemplate } from '@/features/article/templates/ArticleDetailTemplate';
```

**Pagesのインポート:**
```tsx
import { ArticleDetailPage } from '@/features/article/pages/ArticleDetailPage';
```

### 5層構造のメリット

1. **責任の明確化**: 各層の責務が明確で、コードの保守性が向上
2. **再利用性**: Molecule/Atom層は汎用的で、様々な画面で再利用可能
3. **テスト容易性**: 各層が独立しているため、単体テストが容易
4. **レスポンシブ対応の一元化**: Template層でレスポンシブ対応を集中管理
5. **開発効率**: 各層の役割が明確で、実装に迷わない
6. **スケーラビリティ**: 機能追加時に既存のMolecule/Atomを活用できる

### 実装ガイドライン

1. **Page層**: ロジックに集中し、UIは一切持たない
2. **Template層**: レイアウトとレスポンシブ対応に専念
3. **Organism層**: 機能固有のUI表現に専念
4. **Molecule層**: 汎用的な複合コンポーネントとして実装
5. **Atom層**: 最小単位で、他に依存しない
6. **各コンポーネントは単一責任の原則に従う**
7. **propsの型定義を明確にする**
8. **新しいコンポーネントを作る前に、既存のMolecule/Atomで対応できないか確認する**

## コンポーネント分類

### 1. Page層（ロジック層）

**配置:** `src/features/{feature}/pages/`

ページのロジックに責任を持つコンポーネント。UIには一切責任を持たない。

**例:**
- `ArticleDetailPage.tsx` - 記事詳細ページのロジック
- `ArticleFormPage.tsx` - 記事作成・編集ページのロジック
- `LoginPage.tsx` - ログインページのロジック

### 2. Template層（UI構成層）

**配置:** `src/features/{feature}/templates/`

各ページのUI構成とレスポンシブ対応に責任を持つコンポーネント。

**例:**
- `ArticleDetailTemplate.tsx` - 記事詳細のUI構成
- `ArticleFormTemplate.tsx` - 記事作成・編集のUI構成
- `LoginTemplate.tsx` - ログインのUI構成

### 3. Organism層（特化UI層）

**配置:** `src/features/{feature}/organisms/`

各Template特有のUI単位。機能に特化したコンポーネント。

**例:**
- `ArticleDetail.tsx` - 記事詳細表示
- `ArticleForm.tsx` - 記事フォーム
- `ArticleList.tsx` - 記事一覧
- `Header.tsx` (features/global) - ヘッダー
- `Footer.tsx` (features/global) - フッター

### 4. Molecule層（汎用複合層）

**配置:** `src/components/molecules/`

汎用的な複合コンポーネント。Atomsを組み合わせた機能単位。

**例:**
- `EmailField.tsx` - メールアドレス入力
- `PasswordField.tsx` - パスワード入力
- `TagInput.tsx` - タグ入力
- `CharacterCounter.tsx` - 文字数カウンター

### 5. Atom層（汎用最小層）

**配置:** `src/components/ui/`

汎用的な最小単位のUIコンポーネント。アプリケーション全体で再利用される。

**例:**
- `Button.tsx` - ボタン（shadcn/ui）
- `Input.tsx` - 入力フィールド（shadcn/ui）
- `Card.tsx` - カード（shadcn/ui）
- `FormField.tsx` - フォームフィールド
- `ErrorAlert.tsx` - エラーアラート
- `TagChip.tsx` - タグチップ

### 6. グローバル機能のOrganism

**配置:** `src/features/global/organisms/`

アプリケーション全体で使用されるレイアウトコンポーネント。

**例:**
- `Header.tsx` - ヘッダー
- `Footer.tsx` - フッター
- `Sidebar.tsx` - サイドバー

### 7. その他共通コンポーネント

**配置:** `src/components/`

特定の層に属さない、アプリケーション全体で使用される共通のコンポーネント。

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

複雑なロジックや複数のAPIフックを組み合わせる場合は、カスタムフックを作成し、Page層で使用します。

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

**Page層での使用:**
```tsx
// src/features/article/pages/ArticleFormPage.tsx
'use client';

import { useArticleForm } from '../hooks/useArticleForm';
import { ArticleFormTemplate } from '../templates/ArticleFormTemplate';

interface ArticleFormPageProps {
  articleId?: number;
}

export function ArticleFormPage({ articleId }: ArticleFormPageProps) {
  const formState = useArticleForm({
    articleId,
    onSuccess: () => {
      // 成功時の処理
    },
  });

  return <ArticleFormTemplate {...formState} />;
}
```

## ページの実装パターン

### 1. Next.js App Router ページ（Server Component）

App Routerのページファイルは、基本的にfeaturesディレクトリのPageコンポーネントを呼び出すだけのシンプルな実装にします。

```tsx
// src/app/articles/[id]/page.tsx
import { ArticleDetailPage } from '@/features/article/pages/ArticleDetailPage';

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function Page({ params }: PageProps) {
  const { id } = await params;
  const articleId = Number(id);

  return <ArticleDetailPage articleId={articleId} />;
}
```

### 2. Page層の実装（Client Component）

データフェッチングとビジネスロジックを担当します。

```tsx
// src/features/article/pages/ArticleDetailPage.tsx
'use client';

import { useGetArticle } from '@/lib/api';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { Loading } from '@/components/Loading';
import { ErrorMessage } from '@/components/ErrorMessage';

interface ArticleDetailPageProps {
  articleId: number;
}

export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
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

  return <ArticleDetailTemplate article={data} />;
}
```

### 3. Template層の実装

UIの構成とレスポンシブ対応を担当します。

```tsx
// src/features/article/templates/ArticleDetailTemplate.tsx
import { GetArticleResponse } from '@/lib/api/types';
import { ArticleDetail } from '../organisms/ArticleDetail';
import { Container } from '@/components/ui/Container';

interface ArticleDetailTemplateProps {
  article: GetArticleResponse;
}

export function ArticleDetailTemplate({ article }: ArticleDetailTemplateProps) {
  return (
    <Container className="py-8">
      <div className="max-w-4xl mx-auto">
        {/* デスクトップレイアウト */}
        <div className="hidden md:block">
          <ArticleDetail article={article} layout="wide" />
        </div>
        
        {/* モバイルレイアウト */}
        <div className="block md:hidden">
          <ArticleDetail article={article} layout="compact" />
        </div>
      </div>
    </Container>
  );
}
```

### 4. Organism層の実装

機能固有のUI表現を担当します。

```tsx
// src/features/article/organisms/ArticleDetail.tsx
import { GetArticleResponse } from '@/lib/api/types';
import { TagChip } from '@/components/ui/TagChip';
import { Card } from '@/components/ui/Card';

interface ArticleDetailProps {
  article: GetArticleResponse;
  layout?: 'wide' | 'compact';
}

export function ArticleDetail({ article, layout = 'wide' }: ArticleDetailProps) {
  const isWide = layout === 'wide';
  
  return (
    <Card className={isWide ? 'p-8' : 'p-4'}>
      <h1 className={isWide ? 'text-4xl' : 'text-2xl'}>{article.title}</h1>
      <div className="flex items-center gap-4 my-4">
        <span className="text-gray-600">著者: {article.authorName}</span>
        <span className="text-gray-600">いいね: {article.likeCount}</span>
      </div>
      <div className="prose max-w-none">{article.body}</div>
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2 mt-6">
          {article.tags.map((tag, index) => (
            <TagChip key={index} tag={tag} />
          ))}
        </div>
      )}
    </Card>
  );
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

TanStack Queryの `error` を使用し、Page層でハンドリングします。

```tsx
// Page層でのエラーハンドリング
const { data, error } = useGetArticle({ id: articleId });

if (error) {
  return <ErrorMessage message="記事の取得に失敗しました" />;
}
```

### フォームバリデーション

Page層またはカスタムフックで実装します。

```tsx
// Page層でのバリデーション
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

const handleSubmit = () => {
  if (validate()) {
    // 送信処理
  }
};
```

## パフォーマンス最適化

### 1. React.memo

Template、Organism、Molecule層で使用します。

```tsx
import { memo } from 'react';

export const ArticleDetail = memo(function ArticleDetail({ article }: ArticleDetailProps) {
  // ...
});
```

### 2. useCallback

Page層のコールバック関数で使用します。

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

1. **5層構造のAtomic Designパターン**を理解する
   - Page: ロジック層（UIに責任を持たない）
   - Template: UI構成層（レスポンシブ対応）
   - Organism: 特化UI層（機能固有のUI）
   - Molecule: 汎用複合層（再利用可能な複合コンポーネント）
   - Atom: 汎用最小層（最小単位のUIコンポーネント）
2. **各層の責任範囲**を明確に理解する
3. **components/ui**にAtomを配置
4. **components/molecules**にMoleculeを配置
5. **features/{feature}/organisms**に機能固有のOrganismを配置
6. **features/{feature}/templates**にTemplateを配置
7. **features/{feature}/pages**にPageを配置
8. **TanStack Query フック**でデータフェッチング（Page層）
9. **型定義**を活用して型安全に開発

### AIエージェントが生成すべきコード

1. **Atoms**: `src/components/ui/`
2. **Molecules**: `src/components/molecules/`
3. **Organisms**: `src/features/{feature}/organisms/`
4. **Templates**: `src/features/{feature}/templates/`
5. **Pages**: `src/features/{feature}/pages/`
6. **App Router Page**: `src/app/{route}/page.tsx`

### 実装の順序

1. まず必要なAtomsが存在するか確認・作成
2. 次にAtomsを組み合わせてMoleculesを作成
3. MoleculesとAtomsを組み合わせてOrganismsを実装
4. OrganismsをレイアウトしてTemplatesを実装
5. データフェッチングとロジックを含むPagesを実装
6. App RouterのPageコンポーネントからPagesを呼び出す

### 各層の依存関係

```
App Router Page (src/app/)
    ↓ 呼び出し
Page (features/{feature}/pages/)
    ↓ 呼び出し
Template (features/{feature}/templates/)
    ↓ 呼び出し
Organism (features/{feature}/organisms/)
    ↓ 呼び出し
Molecule (components/molecules/)
    ↓ 呼び出し
Atom (components/ui/)
```

### 実装時のチェックリスト

- [ ] 新しいコンポーネントを作る前に、既存のAtom/Moleculeで対応できないか確認
- [ ] ロジックはPage層に集約されているか
- [ ] UI構成とレスポンシブ対応はTemplate層で行われているか
- [ ] Organismは機能固有のUIに専念しているか
- [ ] MoleculeとAtomは汎用的で再利用可能か
- [ ] 各層の責任範囲を超えていないか
- [ ] propsの型定義は明確か
- [ ] 適切なインポートパスを使用しているか

## 関連ドキュメント

- [API使用方法](./api-usage.md)
- [認証戦略](./authentication-strategy.md)
- [アーキテクチャ概要](./architecture-overview.md)
