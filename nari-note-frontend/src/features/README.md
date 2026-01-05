# Features

このディレクトリには、機能ごとにモジュール化されたコンポーネントが配置されます。

## ディレクトリ構造

各機能（feature）は以下の構造を持ちます：

```
{feature}/
├── components/     # Presentational Components（表示専用コンポーネント）
├── containers/     # Container Components（データ管理コンポーネント）
├── hooks/          # カスタムフック（機能固有のロジック）
└── types.ts        # 型定義（機能固有の型）
```

## 機能一覧

### article/
記事に関する機能
- 記事一覧表示
- 記事詳細表示
- 記事作成・編集
- いいね機能

### auth/
認証に関する機能
- ログイン
- サインアップ
- ログアウト

### user/
ユーザーに関する機能
- プロフィール表示
- プロフィール編集

## 新しい機能を追加する場合

1. 機能名のディレクトリを作成（例: `comment/`）
2. 以下のサブディレクトリを作成:
   - `components/` - UI表示コンポーネント
   - `containers/` - データ管理コンポーネント
   - `hooks/` - カスタムフック（必要に応じて）
3. `types.ts` を作成（機能固有の型定義が必要な場合）

## Container/Presentationalパターン

### Container Component
- **配置**: `containers/`
- **責務**: データフェッチング、状態管理、ビジネスロジック
- **ファイル名**: `{ComponentName}Container.tsx`
- **例**: `ArticleListContainer.tsx`

### Presentational Component
- **配置**: `components/`
- **責務**: UI表示のみ
- **ファイル名**: `{ComponentName}.tsx`
- **例**: `ArticleList.tsx`

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) を参照してください。
