# フロントエンドアーキテクチャ図解

nari-note-frontendのアーキテクチャを図解で説明します。

## 全体構造

```
┌─────────────────────────────────────────────────────────────────┐
│                         Browser (User)                          │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Next.js App (Frontend)                       │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │                    App Router (Pages)                     │ │
│  │                  src/app/***/page.tsx                     │ │
│  └─────────────────────────┬─────────────────────────────────┘ │
│                            │                                    │
│                            ↓                                    │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │              Container Components (データ管理)              │ │
│  │          src/features/{feature}/containers/               │ │
│  │                                                            │ │
│  │  • データフェッチング (TanStack Query)                      │ │
│  │  • ビジネスロジック                                        │ │
│  │  • 状態管理                                                │ │
│  └─────────────────────────┬─────────────────────────────────┘ │
│                            │                                    │
│                            ↓                                    │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │         Presentational Components (UI表示)                │ │
│  │          src/features/{feature}/components/               │ │
│  │                                                            │ │
│  │  • UI描画のみ                                              │ │
│  │  • propsを受け取る                                         │ │
│  │  • スタイリング (Tailwind CSS)                            │ │
│  └───────────────────────────────────────────────────────────┘ │
│                                                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Backend API (ASP.NET Core)                   │
└─────────────────────────────────────────────────────────────────┘
```

## ディレクトリ構造とデータフロー

```
src/
│
├── app/                              # 1️⃣ ユーザーがアクセス
│   ├── articles/
│   │   └── [id]/
│   │       └── page.tsx             # ArticleDetailPage
│   │           │
│   │           └─→ 2️⃣ Containerを呼び出し
│
├── features/                         # 機能モジュール
│   └── article/
│       │
│       ├── containers/               # 3️⃣ データ管理
│       │   └── ArticleDetailContainer.tsx
│       │       │
│       │       ├─→ useGetArticle()  # 4️⃣ API呼び出し
│       │       │   (TanStack Query)
│       │       │
│       │       └─→ 5️⃣ Presentationalに渡す
│       │
│       ├── components/               # 6️⃣ UI表示
│       │   └── ArticleDetail.tsx
│       │       └─→ 7️⃣ 画面に表示
│       │
│       └── hooks/                    # カスタムフック
│           └── useArticleForm.ts
│
├── components/                       # 共通コンポーネント
│   ├── ui/                          # Button, Input等
│   ├── layout/                      # Header, Footer等
│   └── common/                      # Loading, ErrorMessage等
│
└── lib/                             # 共通ロジック
    ├── api/                         # API関連
    │   ├── client.ts                # Axiosクライアント
    │   ├── hooks.ts                 # TanStack Query フック
    │   └── types.ts                 # 型定義
    ├── utils/                       # ユーティリティ関数
    ├── hooks/                       # 共通カスタムフック
    └── constants/                   # 定数定義
```

## Container/Presentationalパターン詳細

### データフロー

```
┌──────────────────────────────────────────────────────────────────┐
│                         Page Component                           │
│                    (src/app/articles/[id]/page.tsx)             │
│                                                                  │
│  export default function ArticleDetailPage() {                  │
│    const params = useParams();                                  │
│    const articleId = Number(params.id);                         │
│                                                                  │
│    return <ArticleDetailContainer articleId={articleId} />;    │
│  }                                                               │
└────────────────────────┬─────────────────────────────────────────┘
                         │ articleId を渡す
                         ↓
┌──────────────────────────────────────────────────────────────────┐
│                    Container Component                           │
│    (src/features/article/containers/ArticleDetailContainer.tsx) │
│                                                                  │
│  'use client';  ← Client Component ディレクティブ                │
│                                                                  │
│  export function ArticleDetailContainer({ articleId }) {        │
│    const { data, isLoading, error } = useGetArticle({          │
│      id: articleId                                              │
│    }); ← ④ API呼び出し                                          │
│                                                                  │
│    if (isLoading) return <Loading />;                          │
│    if (error) return <ErrorMessage />;                         │
│    if (!data) return null;                                      │
│                                                                  │
│    return <ArticleDetail article={data} />;                    │
│  }         ↑                         ↓                          │
│            │                    article データを渡す             │
│         ③ 状態管理                                               │
└────────────────────────┬─────────────────────────────────────────┘
                         │
                         ↓
┌──────────────────────────────────────────────────────────────────┐
│                 Presentational Component                         │
│       (src/features/article/components/ArticleDetail.tsx)       │
│                                                                  │
│  export function ArticleDetail({ article }) {  ← ⑤ propsを受取  │
│    return (                                                      │
│      <article>                                                   │
│        <h1>{article.title}</h1>  ← ⑥ データを表示              │
│        <p>{article.authorName}</p>                              │
│        <div>{article.body}</div>                                │
│      </article>                                                  │
│    );                                                            │
│  }                                                               │
└──────────────────────────────────────────────────────────────────┘
```

## API通信フロー

```
┌─────────────────────────────────────────────────────────────────┐
│                      Container Component                        │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓ useGetArticle({ id: 1 })
┌─────────────────────────────────────────────────────────────────┐
│                   TanStack Query Hook                           │
│                  (src/lib/api/hooks.ts)                         │
│                                                                 │
│  • キャッシュチェック                                            │
│  • データ取得                                                    │
│  • 自動リフェッチ                                                │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓ articlesApi.getArticle({ id: 1 })
┌─────────────────────────────────────────────────────────────────┐
│                    API Endpoint Function                        │
│                  (src/lib/api/endpoints.ts)                     │
│                                                                 │
│  getArticle: async ({ id }) => {                               │
│    const { data } = await apiClient.get(`/api/articles/${id}`)│
│    return data;                                                 │
│  }                                                              │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓ GET /api/articles/1
┌─────────────────────────────────────────────────────────────────┐
│                       Axios Client                              │
│                   (src/lib/api/client.ts)                       │
│                                                                 │
│  • ベースURL設定                                                 │
│  • 認証トークン自動付与                                          │
│  • エラーハンドリング                                            │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓ HTTP Request
┌─────────────────────────────────────────────────────────────────┐
│                    Backend API                                  │
│                  (ASP.NET Core)                                 │
└─────────────────────────────────────────────────────────────────┘
```

## 機能追加のフロー

### 新機能「コメント機能」を追加する例

```
Step 1: ディレクトリ作成
───────────────────────
src/features/comment/
├── components/
├── containers/
└── hooks/


Step 2: 型定義（必要に応じて）
─────────────────────────────
src/features/comment/types.ts


Step 3: Presentationalコンポーネント作成
──────────────────────────────────────
src/features/comment/components/
├── CommentList.tsx        # コメント一覧表示
├── CommentItem.tsx        # コメント1件表示
└── CommentForm.tsx        # コメント投稿フォーム


Step 4: Containerコンポーネント作成
──────────────────────────────────
src/features/comment/containers/
├── CommentListContainer.tsx
├── CommentItemContainer.tsx
└── CommentFormContainer.tsx


Step 5: カスタムフック作成（必要に応じて）
──────────────────────────────────────
src/features/comment/hooks/
└── useCommentForm.ts      # フォームロジック


Step 6: ページから使用
─────────────────────
src/app/articles/[id]/page.tsx

import { CommentListContainer } from '@/features/comment/containers/CommentListContainer';

export default function ArticleDetailPage() {
  return (
    <>
      <ArticleDetailContainer articleId={articleId} />
      <CommentListContainer articleId={articleId} />
    </>
  );
}
```

## 共通コンポーネントの活用

```
機能コンポーネント（feature）
    ↓ 使用
共通コンポーネント（components）

┌─────────────────────────────────────────────────┐
│     ArticleDetailContainer.tsx                  │
│                                                 │
│  if (isLoading) return <Loading />;      ←─┐   │
│  if (error) return <ErrorMessage />;     ←─┤   │
│  if (!data) return <EmptyState />;       ←─┤   │
└─────────────────────────────────────────────┼───┘
                                              │
                                              │ import
                                              │
┌─────────────────────────────────────────────┼───┐
│  src/components/common/                     │   │
│  ├── Loading.tsx          ←─────────────────┘   │
│  ├── ErrorMessage.tsx     ←─────────────────┐   │
│  └── EmptyState.tsx       ←─────────────────┘   │
└─────────────────────────────────────────────────┘

これにより、
• コードの再利用性が向上
• 一貫したUI/UX
• メンテナンスが容易
```

## スタイリングの流れ

```
Tailwind CSS クラス
    ↓
コンポーネントに適用
    ↓
ビルド時にCSSが生成
    ↓
ブラウザで表示

┌──────────────────────────────────────────────┐
│  ArticleCard.tsx                             │
│                                              │
│  <div className="                            │
│    bg-white              ← 背景白            │
│    rounded-lg            ← 角丸              │
│    shadow                ← 影                │
│    p-6                   ← パディング         │
│    hover:shadow-lg       ← ホバー時に影強調   │
│    transition-shadow     ← トランジション     │
│  ">                                          │
│    ...                                       │
│  </div>                                      │
└──────────────────────────────────────────────┘
```

## 型安全性の確保

```
API型定義（バックエンドから生成）
    ↓
src/lib/api/types.ts
    ↓
コンポーネントで使用

┌──────────────────────────────────────────────┐
│  types.ts                                    │
│                                              │
│  export interface GetArticleResponse {      │
│    id: number;                              │
│    title: string;                           │
│    body: string;                            │
│    authorName: string;                      │
│    likeCount: number;                       │
│    tags: string[];                          │
│  }                                           │
└────────────────┬─────────────────────────────┘
                 │ import
                 ↓
┌──────────────────────────────────────────────┐
│  ArticleDetail.tsx                           │
│                                              │
│  interface ArticleDetailProps {             │
│    article: GetArticleResponse;  ← 型安全   │
│  }                                           │
│                                              │
│  export function ArticleDetail({            │
│    article                                   │
│  }: ArticleDetailProps) {                   │
│    return <h1>{article.title}</h1>;        │
│  }              ↑                           │
│                 └─ TypeScriptが型チェック    │
└──────────────────────────────────────────────┘
```

## まとめ

### アーキテクチャの3つの柱

1. **明確な分離**
   - Container (データ) と Presentational (UI) の分離
   - 機能ごとのモジュール化 (features/)
   - 共通コンポーネントの再利用

2. **型安全性**
   - TypeScriptによる型チェック
   - APIレスポンスの型定義
   - propsの型定義

3. **保守性**
   - 一貫したディレクトリ構造
   - 明確な命名規則
   - 包括的なドキュメント

この構造により、開発者もAIも**どこに何を書けば良いかが明確**になります。

## 参考ドキュメント

- [フロントエンドアーキテクチャガイド](./frontend-architecture.md)
- [フロントエンド実装ガイド](./frontend-implementation-guide.md)
- [クイックリファレンス](./frontend-quick-reference.md)
