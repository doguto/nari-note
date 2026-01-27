# Article Feature

記事に関する機能を提供するモジュールです。

**重要**: Atomic Designパターンに従い、Page/Template/Organism構造で実装されています。

## ディレクトリ構造

```
article/
├── pages/                  # Pages（ロジック層）
│   ├── ArticleFormPage.tsx
│   ├── ArticleDetailPage.tsx
│   ├── HomeArticleListPage.tsx
│   ├── MyArticlesListPage.tsx
│   ├── DraftArticleListPage.tsx
│   ├── ArticleSearchPage.tsx
│   └── index.ts
├── templates/              # Templates（UI構成層）
│   ├── ArticleFormTemplate.tsx
│   ├── ArticleDetailTemplate.tsx
│   ├── HomeArticleListTemplate.tsx
│   ├── MyArticlesListTemplate.tsx
│   ├── DraftArticleListTemplate.tsx
│   ├── ArticleSearchTemplate.tsx
│   └── index.ts
├── organisms/              # Organisms（UI単位）
│   ├── ArticleTitleInput.tsx
│   ├── ArticleBodyEditor.tsx
│   ├── ArticleFormActions.tsx
│   ├── CommentForm.tsx
│   ├── CommentList.tsx
│   ├── PublishSettingsDialog.tsx
│   ├── TipTapEditor.tsx
│   └── index.ts
└── README.md
```

## アーキテクチャ概要

### Page/Template/Organism構造

article featureは、以下の3層構造で実装されています：

#### 1. Page（ロジック層）
- データフェッチング（TanStack Query）
- 状態管理（useState, useEffect）
- ビジネスロジック（バリデーション、イベントハンドリング）
- UIコンポーネントは**含まない**
- Templateコンポーネントにpropsを渡す

#### 2. Template（UI構成層）
- レイアウトとレスポンシブ対応
- Organism/Moleculeを組み合わせてUIを構築
- propsから受け取ったデータとハンドラーを使用
- ロジックは**含まない**

#### 3. Organism（UI単位）
- Template特有の再利用可能なUI単位
- 完全な機能を持つコンポーネント
- 他のfeatureからも使用可能

## コンポーネント一覧

### Pages

#### ArticleFormPage
記事作成・編集ページのロジック管理
- 記事データのCRUD操作
- フォーム状態管理
- バリデーション
- ページ遷移制御

#### ArticleDetailPage
記事詳細ページのロジック管理
- 記事データ取得
- いいね機能
- コメント管理
- 認証状態の判定

#### HomeArticleListPage
ホーム画面の記事一覧のロジック管理
- 記事一覧データ取得
- エラーハンドリング

#### MyArticlesListPage
マイ記事一覧ページのロジック管理
- ユーザーの記事取得
- タブ切り替え（公開済み/下書き）
- 記事削除処理

#### DraftArticleListPage
下書き記事一覧ページのロジック管理
- 下書き記事取得
- 記事削除処理

#### ArticleSearchPage
記事検索ページのロジック管理
- 検索クエリ管理
- 検索結果取得

### Templates

各PageコンポーネントのUI構成とレイアウトを担当します。

- **ArticleFormTemplate** - 記事フォームのレイアウト
- **ArticleDetailTemplate** - 記事詳細のレイアウト
- **HomeArticleListTemplate** - ホーム記事一覧のレイアウト
- **MyArticlesListTemplate** - マイ記事一覧のレイアウト
- **DraftArticleListTemplate** - 下書き一覧のレイアウト
- **ArticleSearchTemplate** - 検索ページのレイアウト

### Organisms

#### ArticleTitleInput
記事タイトル入力フィールド

#### ArticleBodyEditor
記事本文エディタ（マークダウン）
- TipTapEditorを使用
- 文字数制限表示

#### ArticleFormActions
記事フォームのアクションボタン
- 保存ボタン
- 公開設定ボタン

#### CommentForm
コメント投稿フォーム

#### CommentList
コメント一覧表示

#### PublishSettingsDialog
記事の投稿設定ダイアログ
- 即座に公開または予約投稿を選択可能
- 日時選択とバリデーション機能
- ISO 8601形式（UTC）でAPIに送信

#### TipTapEditor
リッチテキストエディタ（TipTap使用）

## 使用方法

### ページから使用する

```tsx
// src/app/(with-layout)/page.tsx
import { HomeArticleListPage } from '@/features/article/pages';

export default function Home() {
  return (
    <div>
      <h2>新着記事</h2>
      <HomeArticleListPage />
    </div>
  );
}
```

```tsx
// src/app/(with-layout)/articles/new/page.tsx
import { ArticleFormPage } from '@/features/article/pages';

export default function NewArticlePage() {
  return (
    <div className="container">
      <h1>新規記事作成</h1>
      <ArticleFormPage />
    </div>
  );
}
```

```tsx
// src/app/(with-layout)/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailPage } from '@/features/article/pages';

export default function ArticleDetailPageRoute() {
  const params = useParams();
  const articleId = Number(params.id);

  return <ArticleDetailPage articleId={articleId} />;
}
```

## Atomic Design構成

### 使用しているAtoms（共通コンポーネント）
- `FormField` - 基本フォームフィールド
- `ErrorAlert` - エラー表示
- `LoadingSpinner` - ローディング表示
- `LikeButton` - いいねボタン
- `EmptyState` - 空状態表示

### 使用しているMolecules（共通コンポーネント）
- `TagInput` - タグ入力
- `SearchBar` - 検索バー
- `ArticleCard` - 記事カード
- `DraftArticleCard` - 下書き記事カード
- `PublishedArticleCard` - 公開済み記事カード

### Organisms（feature固有）
- `ArticleTitleInput`
- `ArticleBodyEditor`
- `ArticleFormActions`
- `CommentForm`
- `CommentList`
- `PublishSettingsDialog`
- `TipTapEditor`

### Templates（UI構成）
- 各ページのUI構成とレイアウト

### Pages（ロジック）
- 各ページのビジネスロジックとデータ管理

## 開発ガイドライン

### 新しいページを追加する場合

1. **Page**を作成（`pages/`）
   - データフェッチング
   - 状態管理
   - ビジネスロジック

2. **Template**を作成（`templates/`）
   - UI構成
   - レスポンシブ対応

3. 必要に応じて**Organism**を作成（`organisms/`）
   - 再利用可能なUI単位

4. `pages/index.ts`と`templates/index.ts`にexportを追加

5. app/配下のルートファイルからPageをimport

### コンポーネント間のデータフロー

```
Page → Template → Organism → Molecule → Atom
  ↓       ↓         ↓          ↓        ↓
ロジック  構成    UI単位     機能     基本要素
```

## 型定義

APIレスポンス型は `@/lib/api/types` からインポートして使用します：

- `ArticleDto` - 基本的な記事データ
- `GetArticleResponse` - 記事詳細（コメント、いいね情報含む）
- `CreateArticleRequest` - 記事作成リクエスト
- `UpdateArticleRequest` - 記事更新リクエスト
