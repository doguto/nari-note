# フロントエンドドキュメント

このディレクトリには、nari-note-frontendの設計・実装ドキュメントが格納されています。

## ドキュメント一覧

### 📚 実装ガイド

#### [implementation-guide.md](./implementation-guide.md) ⭐ 最重要
**フロントエンド実装ガイド（AIエージェント向け）**

このドキュメントは、AIエージェントがnari-note-frontendのコードを生成する際の具体的なガイドラインです。

**重要**: nari-noteではAtomic Designパターンを採用しています。コンポーネントを小さな単位（Atoms → Molecules → Organisms）で実装してください。

**内容:**
- コード生成の基本ルール
  - Atomic Designパターンの使用
  - Container/Presentationalパターン
  - 型定義
  - 共通コンポーネントの活用
- ディレクトリ配置ルール
- 命名規則
- コンポーネント生成パターン
  - Atomの作成
  - Moleculeの作成
  - Organism（シンプルなフォーム）の作成
  - Organism（データ取得を伴う）の作成
  - 複雑なOrganism（フォーム + カスタムフック）
- よくあるパターン
- チェックリスト

**新しいコンポーネントを作成する際は、まずこのドキュメントを参照してください。**

### 🏗️ アーキテクチャ

#### [architecture.md](./architecture.md) ⭐ 重要
**フロントエンドアーキテクチャガイド**

nari-note-frontendの実装時に参照するアーキテクチャの詳細ガイドです。

**内容:**
- 概要
  - Container/Presentationalパターン
  - Atomic Designパターン
- 技術スタック
  - Next.js 15, React 19, TypeScript
  - TanStack Query (React Query)
  - Tailwind CSS 4
  - Axios
- ディレクトリ構造
- 各レイヤーの責務
- デザインパターン
  - Atomic Design
  - Container/Presentational
  - Feature-based
- データフェッチング戦略
- 状態管理戦略
- スタイリング規約
- パフォーマンス最適化

**アーキテクチャの全体像を理解する際は、このドキュメントを参照してください。**

#### [architecture-diagram.md](./architecture-diagram.md)
**フロントエンドアーキテクチャ図**

アーキテクチャを視覚的に理解するための図解です。

**内容:**
- コンポーネント階層図
- データフロー図
- ディレクトリ構造図

### 🔌 API使用方法

#### [api-usage.md](./api-usage.md) ⭐ 重要
**フロントエンドAPI使用方法**

バックエンドAPIとの通信方法の詳細ガイドです。

**内容:**
- TanStack Query (React Query) の使用方法
- API関数の定義（`src/lib/api/`）
- カスタムフックの使用
  - データ取得（Query）
  - データ変更（Mutation）
- エラーハンドリング
- 認証トークンの管理
- キャッシング戦略
- 実装例

**バックエンドAPIを呼び出す際は、このドキュメントを参照してください。**

### 📖 クイックリファレンス

#### [quick-reference.md](./quick-reference.md)
**フロントエンドクイックリファレンス**

よく使うパターンやコードスニペットのクイックリファレンスです。

**内容:**
- よく使うコンポーネントパターン
- カスタムフックの使い方
- スタイリングのベストプラクティス
- 便利なユーティリティ関数
- トラブルシューティング

**実装中にすぐに確認したい情報は、このドキュメントを参照してください。**

## ドキュメントの使い方

### 新しいコンポーネントを作成する場合
1. **[implementation-guide.md](./implementation-guide.md)** でAtomic Designパターンとコンポーネント生成パターンを確認 ⭐
2. **[architecture.md](./architecture.md)** でディレクトリ構造と配置ルールを確認
3. **[quick-reference.md](./quick-reference.md)** でよく使うパターンを確認

### バックエンドAPIを呼び出す場合
1. **[api-usage.md](./api-usage.md)** でTanStack Queryの使い方を確認 ⭐
2. **[implementation-guide.md](./implementation-guide.md)** でContainer/Presentationalパターンを確認

### アーキテクチャを理解する場合
1. **[architecture.md](./architecture.md)** でアーキテクチャの全体像を確認
2. **[architecture-diagram.md](./architecture-diagram.md)** で視覚的に理解

### よく使うパターンを確認する場合
1. **[quick-reference.md](./quick-reference.md)** でクイックリファレンスを参照

## AI（GitHub Copilot）向けの情報

これらのドキュメントは、AI開発支援ツールが自動的に参照し、コード生成やレビューに活用できるように構造化されています。

**特に重要なドキュメント:**
- **[implementation-guide.md](./implementation-guide.md)** - 実装パターンの完全なガイド ⭐
- **[architecture.md](./architecture.md)** - アーキテクチャの詳細ガイド ⭐
- **[api-usage.md](./api-usage.md)** - API使用方法の完全なガイド ⭐
- **[quick-reference.md](./quick-reference.md)** - クイックリファレンス

**実装の優先順位:**
1. まず **[implementation-guide.md](./implementation-guide.md)** でAtomic Designパターンとコンポーネント生成パターンを把握
2. 次に **[api-usage.md](./api-usage.md)** でAPI使用方法を理解
3. **[architecture.md](./architecture.md)** でアーキテクチャの全体像を理解
4. 必要に応じて **[quick-reference.md](./quick-reference.md)** でクイックリファレンスを参照

## Atomic Designの階層

nari-noteではAtomic Designパターンを採用しています：

```
Atoms（原子）
  ↓ 組み合わせ
Molecules（分子）
  ↓ 組み合わせ
Organisms（有機体）
  ↓ 組み合わせ
Pages（ページ）
```

**コンポーネントの配置:**
- **Atoms**: `src/components/common/atoms/`
- **Molecules**: `src/components/common/molecules/`
- **Organisms**: `src/features/{feature}/organisms/`
- **Pages**: `src/app/`

## 関連リンク

- [バックエンドドキュメント](../../nari-note-backend/Documents/README.md)
- [プロジェクトルートREADME](../../README.md)
