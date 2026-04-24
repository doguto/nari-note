# Nari-Note フロントエンド

技術記事の投稿・共有プラットフォーム「なりノート」のフロントエンド実装です。

## 技術スタック

- **フレームワーク**: Next.js 15
- **言語**: TypeScript
- **スタイリング**: TailwindCSS
- **データフェッチング**: TanStack Query (React Query)
- **HTTPクライアント**: Axios
- **アーキテクチャ**: Container/Presentationalパターン

## API通信

このプロジェクトでは、TanStack QueryとAxiosを使用してバックエンドAPIと通信します。

### 主な機能

- **型安全**: TypeScriptによる完全な型定義
- **自動キャッシング**: スマートなデータキャッシング戦略
- **認証管理**: 自動的なトークン管理
- **エラーハンドリング**: 統一されたエラー処理

詳細な使い方は[API使用ガイド](./docs/api-usage.md)を参照してください。

## ディレクトリ構造

```
src/
├── app/                    # Next.js App Router
│   ├── page.tsx           # トップページ
│   ├── debug/             # デバッグページ（従来のfetch API）
│   └── debug-new/         # デバッグページ（TanStack Query版）
└── lib/
    ├── api/               # API関連
    │   ├── client.ts      # Axiosクライアント設定
    │   ├── types.ts       # 型定義
    │   ├── endpoints.ts   # API関数
    │   ├── hooks.ts       # TanStack Queryフック
    │   └── index.ts       # エクスポート
    └── providers/         # Providerコンポーネント
        └── QueryProvider.tsx
```

## ドキュメント

- [API使用ガイド](./docs/api-usage.md) - API関数の詳細な使い方
- [フロントエンド実装ガイド](./docs/implementation-guide.md) - Atomic Designとコンポーネント生成パターン
- [フロントエンドアーキテクチャ](./docs/architecture.md) - アーキテクチャの詳細ガイド
- [クイックリファレンス](./docs/quick-reference.md) - よく使うパターン集
