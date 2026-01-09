# Article Feature

記事に関する機能を提供するモジュールです。

**重要**: Atomic Designパターンに従い、完全な機能ブロックは`organisms/`ディレクトリに配置されます。

## ディレクトリ構造

```
article/
├── organisms/              # Organisms（完全な機能ブロック）
│   ├── ArticleFormPage.tsx
│   ├── ArticleDetailPage.tsx
│   ├── ArticleDetailContainer.tsx
│   └── HomeArticleList.tsx
└── types.ts               # 型定義（必要に応じて）
```

## Organisms一覧

### ArticleFormPage.tsx
記事作成・編集フォーム。Atoms/Moleculesを組み合わせた完全な機能。

- TitleField（Molecule）を使用
- BodyField（Molecule）を使用
- TagInput（Molecule）を使用
- データフェッチングとビジネスロジックを含む

### ArticleDetailPage.tsx
記事詳細表示。Presentational Organism。

- TagChip（Atom）を使用
- propsで記事データを受け取って表示

### ArticleDetailContainer.tsx
記事詳細のデータフェッチング（オプション）。

**使用ガイドライン**:
- **シンプルな場合**: ArticleDetailPage内で直接データフェッチング
- **複雑な場合**: Containerに分離してArticleDetailPageにデータを渡す

現在の実装ではArticleDetailPageが直接データフェッチングを行っています。将来的に複雑化した場合にContainerに分離することを検討してください。

### HomeArticleList.tsx
ホームページの記事一覧表示。

## 使用方法

### ページから記事詳細を表示

```tsx
// src/app/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailContainer } from '@/features/article/organisms/ArticleDetailContainer';

export default function ArticleDetailPage() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <div className="container mx-auto">
      <ArticleDetailContainer articleId={articleId} />
    </div>
  );
}
```

### ページから記事作成フォームを表示

```tsx
// src/app/articles/new/page.tsx
import { ArticleFormPage } from '@/features/article/organisms/ArticleFormPage';

export default function NewArticlePage() {
  return (
    <div className="container mx-auto max-w-4xl">
      <ArticleFormPage />
    </div>
  );
}
```

## Atomic Design構成

### 使用しているAtoms
- `FormField` - 基本フォームフィールド
- `ErrorAlert` - エラー表示
- `TagChip` - タグ表示

### 使用しているMolecules
- `TitleField` - タイトル入力（FormField + CharacterCounter）
- `BodyField` - 本文入力
- `TagInput` - タグ入力（Input + Button + TagChip）

### Organisms
- `ArticleFormPage` - 完全な記事フォーム
- `ArticleDetailPage` - 完全な記事詳細表示
- `ArticleDetailContainer` - データフェッチング + ArticleDetailPage

## 型定義

`types.ts` に記事機能固有の型を定義します。
APIレスポンス型は `@/lib/api/types` からインポートして使用します。
