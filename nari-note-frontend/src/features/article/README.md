# Article Feature

記事に関する機能を提供するモジュールです。

## コンポーネント一覧

### Presentational Components（`components/`）

表示のみを担当するコンポーネント。propsを受け取ってUIを描画します。

- **ArticleCard.tsx** - 記事カード（一覧で使用）
- **ArticleList.tsx** - 記事一覧
- **ArticleDetail.tsx** - 記事詳細
- **ArticleForm.tsx** - 記事作成・編集フォーム

### Container Components（`containers/`）

データフェッチングとビジネスロジックを担当するコンポーネント。

- **HomeArticleListContainer.tsx** - ホーム画面の記事一覧のデータ管理
- **ArticleDetailContainer.tsx** - 記事詳細のデータ管理
- **ArticleFormContainer.tsx** - 記事フォームのデータ管理

### Custom Hooks（`hooks/`）

記事機能固有のカスタムフック。

- **useArticleForm.ts** - 記事フォームのロジック管理

## 使用方法

### 記事一覧を表示

```tsx
import { ArticleListContainer } from '@/features/article/containers/ArticleListContainer';

export default function ArticlesPage() {
  return <ArticleListContainer />;
}
```

### 記事詳細を表示

```tsx
import { ArticleDetailContainer } from '@/features/article/containers/ArticleDetailContainer';

export default function ArticleDetailPage({ params }: { params: { id: string } }) {
  return <ArticleDetailContainer articleId={Number(params.id)} />;
}
```

### 記事作成フォーム

```tsx
import { ArticleFormContainer } from '@/features/article/containers/ArticleFormContainer';

export default function NewArticlePage() {
  return <ArticleFormContainer />;
}
```

## 型定義

`types.ts` に記事機能固有の型を定義します。
APIレスポンス型は `@/lib/api/types` からインポートして使用します。
