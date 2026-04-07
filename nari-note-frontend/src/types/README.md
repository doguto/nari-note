# Types

このディレクトリには、アプリケーション全体で使用されるグローバルな型定義が配置されます。

## 概要

グローバルに使用される型定義や、複数の機能にまたがる共通の型を定義します。

## ファイル例

### index.ts
グローバル型定義

```tsx
// 共通のID型
export type ID = number;

// ページネーション型
export interface Pagination {
  page: number;
  perPage: number;
  total: number;
  totalPages: number;
}

// ソート順
export type SortOrder = 'asc' | 'desc';

// エラー型
export interface AppError {
  message: string;
  code?: string;
  statusCode?: number;
}

// フォームフィールド型
export interface FormField<T = string> {
  value: T;
  error?: string;
  touched: boolean;
}

// ローディング状態
export interface LoadingState {
  isLoading: boolean;
  error: AppError | null;
}
```

## 使用例

```tsx
import type { ID, Pagination, AppError } from '@/types';

interface ArticleListProps {
  articles: Array<{ id: ID; title: string }>;
  pagination: Pagination;
  error: AppError | null;
}
```

## 型定義の配置ルール

### このディレクトリに配置する型

- **グローバル型**: アプリケーション全体で使用される型
- **共通インターフェース**: 複数の機能で共有される型
- **ユーティリティ型**: 汎用的な型定義

### 他の場所に配置する型

- **API型**: `src/lib/api/types.ts`
- **機能固有の型**: `src/features/{feature}/types.ts`
- **コンポーネントProps型**: 各コンポーネントファイル内

## 型定義のベストプラクティス

### 1. 明確な命名

```tsx
// Good
export interface UserProfile { ... }
export interface ArticleDetail { ... }

// Bad
export interface User { ... }  // 曖昧
export interface Data { ... }  // 汎用的すぎる
```

### 2. エクスポートを明示

```tsx
// 全てexportする
export interface Pagination { ... }
export type SortOrder = 'asc' | 'desc';
```

### 3. ドキュメントコメントを追加

```tsx
/**
 * ページネーション情報
 * @property page - 現在のページ番号（1始まり）
 * @property perPage - 1ページあたりの件数
 * @property total - 総件数
 * @property totalPages - 総ページ数
 */
export interface Pagination {
  page: number;
  perPage: number;
  total: number;
  totalPages: number;
}
```

### 4. 型の再利用

```tsx
// 基本型を定義
export interface BaseEntity {
  id: ID;
  createdAt: Date;
  updatedAt: Date;
}

// 拡張して使用
export interface Article extends BaseEntity {
  title: string;
  body: string;
}
```

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/architecture.md) を参照してください。
