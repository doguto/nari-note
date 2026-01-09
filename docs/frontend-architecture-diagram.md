# フロントエンドアーキテクチャ図解

nari-note-frontendのアーキテクチャを図解で説明します。

**注意**: このドキュメントは古いContainer/Presentational構造を示しています。現在はAtomic Designパターン（Atoms → Molecules → Organisms）を採用しています。最新のアーキテクチャについては [frontend-architecture.md](./frontend-architecture.md) を参照してください。

## 全体構造

```mermaid
graph TB
    Browser["Browser (User)"]
    
    subgraph Frontend["Next.js App (Frontend)"]
        Pages["App Router (Pages)<br/>src/app/***/page.tsx"]
        Container["Container Components (データ管理)<br/>src/features/{feature}/containers/<br/><br/>• データフェッチング (TanStack Query)<br/>• ビジネスロジック<br/>• 状態管理"]
        Presentational["Presentational Components (UI表示)<br/>src/features/{feature}/components/<br/><br/>• UI描画のみ<br/>• propsを受け取る<br/>• スタイリング (Tailwind CSS)"]
    end
    
    Backend["Backend API (ASP.NET Core)"]
    
    Browser --> Pages
    Pages --> Container
    Container --> Presentational
    Frontend --> Backend
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

```mermaid
sequenceDiagram
    participant Page as Page Component
    participant Container as Container Component
    participant Hook as useGetArticle()
    participant Presentational as Presentational Component
    
    Note over Page: src/app/articles/[id]/page.tsx<br/>① ユーザーがアクセス
    
    Page->>Container: ② articleIdを渡す
    
    Note over Container: src/features/article/containers/<br/>ArticleDetailContainer.tsx
    
    Container->>Hook: ③ API呼び出し
    Note over Hook: TanStack Query<br/>データフェッチング
    Hook-->>Container: ④ データ返却
    
    Note over Container: 状態管理<br/>(Loading/Error/Data)
    
    Container->>Presentational: ⑤ articleデータを渡す
    
    Note over Presentational: src/features/article/components/<br/>ArticleDetail.tsx<br/>⑥ UIを表示
```

## API通信フロー

```mermaid
graph TB
    Container["Container Component"]
    TanStackQuery["TanStack Query Hook<br/>src/lib/api/hooks.ts<br/><br/>• キャッシュチェック<br/>• データ取得<br/>• 自動リフェッチ"]
    APIEndpoint["API Endpoint Function<br/>src/lib/api/endpoints.ts<br/><br/>getArticle: async ({ id }) => {<br/>  const { data } = await apiClient.get(`/api/articles/${dollar}{id}`)<br/>  return data;<br/>}"]
    AxiosClient["Axios Client<br/>src/lib/api/client.ts<br/><br/>• ベースURL設定<br/>• 認証トークン自動付与<br/>• エラーハンドリング"]
    Backend["Backend API<br/>(ASP.NET Core)"]
    
    Container -->|"useGetArticle({ id: 1 })"| TanStackQuery
    TanStackQuery -->|"articlesApi.getArticle({ id: 1 })"| APIEndpoint
    APIEndpoint -->|"GET /api/articles/1"| AxiosClient
    AxiosClient -->|"HTTP Request"| Backend
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

```mermaid
graph TB
    Feature["機能コンポーネント (feature)"]
    Common["共通コンポーネント (components)"]
    
    subgraph ArticleContainer["ArticleDetailContainer.tsx"]
        LoadingUse["if (isLoading) return &lt;Loading /&gt;;"]
        ErrorUse["if (error) return &lt;ErrorMessage /&gt;;"]
        EmptyUse["if (!data) return &lt;EmptyState /&gt;;"]
    end
    
    subgraph CommonComponents["src/components/common/"]
        Loading["Loading.tsx"]
        ErrorMessage["ErrorMessage.tsx"]
        EmptyState["EmptyState.tsx"]
    end
    
    Feature --> Common
    LoadingUse -.-> Loading
    ErrorUse -.-> ErrorMessage
    EmptyUse -.-> EmptyState
    
    Note1["これにより、<br/>• コードの再利用性が向上<br/>• 一貫したUI/UX<br/>• メンテナンスが容易"]
```

## スタイリングの流れ

```mermaid
graph LR
    TailwindClass["Tailwind CSS クラス"]
    Component["コンポーネントに適用"]
    Build["ビルド時にCSSが生成"]
    Browser["ブラウザで表示"]
    
    TailwindClass --> Component
    Component --> Build
    Build --> Browser
    
    subgraph Example["ArticleCard.tsx"]
        Code["&lt;div className=&quot;<br/>  bg-white ← 背景白<br/>  rounded-lg ← 角丸<br/>  shadow ← 影<br/>  p-6 ← パディング<br/>  hover:shadow-lg ← ホバー時に影強調<br/>  transition-shadow ← トランジション<br/>&quot;&gt;<br/>  ...<br/>&lt;/div&gt;"]
    end
```

## 型安全性の確保

```mermaid
graph TB
    Backend["API型定義<br/>(バックエンドから生成)"]
    Types["src/lib/api/types.ts"]
    Component["コンポーネントで使用"]
    
    Backend --> Types
    Types --> Component
    
    subgraph TypesDef["types.ts"]
        TypeCode["export interface GetArticleResponse {<br/>  id: number;<br/>  title: string;<br/>  body: string;<br/>  authorName: string;<br/>  likeCount: number;<br/>  tags: string[];<br/>}"]
    end
    
    subgraph ComponentDef["ArticleDetail.tsx"]
        ComponentCode["interface ArticleDetailProps {<br/>  article: GetArticleResponse; ← 型安全<br/>}<br/><br/>export function ArticleDetail({<br/>  article<br/>}: ArticleDetailProps) {<br/>  return &lt;h1&gt;{article.title}&lt;/h1&gt;;<br/>} ↑<br/>  └─ TypeScriptが型チェック"]
    end
    
    Types -.->|import| ComponentDef
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
