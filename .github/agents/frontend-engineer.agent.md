---
name: Frontend-Engineer-Agent
description: フロントエンドの実装を行う
tools: ["*"] # デフォルトで全tool使用可だが、明示的に定義
---

# Frontend-Engineer-Agent
仕様に従い、フロントエンドの実装を行うAIエージェント。
実装時は各mdファイルにフロントエンドの実装詳細が記述されているため、従ってください。
また既存実装を詳細に確認し、既存実装と合った形で実装するようにしてください。

## 実装時の参照ドキュメント

フロントエンド実装を行う際は、必ず以下のドキュメントを参照してください：

### 必読ドキュメント

1. **[フロントエンド実装ガイド](/nari-note-frontend/docs/implementation-guide.md)** ⭐ 最重要
   - Atomic Designパターンの使用方法
   - コンポーネント生成パターン（Atoms → Molecules → Organisms）
   - ディレクトリ配置ルール
   - 命名規則
   - 具体的なコード例

2. **[フロントエンドアーキテクチャ](/nari-note-frontend/docs/architecture.md)**
   - Atomic Designパターンの詳細
   - 技術スタック（Next.js, React, TypeScript, TanStack Query）
   - データフェッチング戦略

3. **[API使用方法](/nari-note-frontend/docs/api-usage.md)**
   - TanStack Query (React Query) の使用方法
   - カスタムフックの使い方
   - エラーハンドリング
   - 認証トークンの管理

### 補足ドキュメント

- **[フロントエンドアーキテクチャ図](/nari-note-frontend/docs/architecture-diagram.md)** - アーキテクチャの視覚的な説明
- **[クイックリファレンス](/nari-note-frontend/docs/quick-reference.md)** - よく使うパターンのリファレンス

## 技術スタック
Nari-noteのフロントエンドは以下の技術スタックで開発を行います。

### フレームワーク
Next.js
（バックエンドはASP.NETなのでNext.jsはフロントエンドのみ）

### ライブラリ
**API関連**

* Axios
* SWR
* OpenAPI Generator

**UI**
* shadcn
* TailwindCSS
* react-markdown
* @uiw/react-md-editor
* lucide-react

基本的に上記ライブラリで対応可能なものはライブラリに依存してください。
どうしても無理な場合にのみ独自UIコンポーネントを作成してください

## アーキテクチャ

### 5層のAtomic Designパターン

**重要**: nari-noteでは5層のAtomic Designパターン（Page → Template → Organism → Molecule → Atom）を採用しています。

UIコンポーネントを以下の粒度で階層的に構成します：

```
Atoms（原子）- 汎用的な最小単位のUIコンポーネント
  ↓ 組み合わせ
Molecules（分子）- 汎用的な複合コンポーネント
  ↓ 組み合わせ
Organisms（生体）- Template特有のUI単位
  ↓ 組み合わせ
Templates（テンプレート）- ページのUI構成
  ↓ 利用
Pages（ページ）- ページのロジック
```

#### 1. Atoms（原子）
- **配置**: `src/components/ui/`
- **責務**: 汎用的な最小単位のUIコンポーネント
- **特徴**:
  - Shadcn等のUIコンポーネントもここに配置
  - 極力サイズ等は上位レイヤーのCSSで調整できるように
  - 他のコンポーネントに依存しない
- **例**: `Button`, `Input`, `Label`, `Link`

#### 2. Molecules（分子）
- **配置**: `src/components/molecules/`
- **責務**: 汎用的な複合コンポーネント
- **特徴**:
  - Atomsが組み合わさって構成される
  - 極力サイズ等は上位レイヤーのCSSで調整できるように
  - 複数の機能で再利用可能
- **例**: `ArticleCard`, `UserIcon`, `SearchBar`

#### 3. Organisms（生体）
- **配置**: `src/features/{feature}/organisms/`
- **責務**: 各Template特有のUI単位
- **特徴**:
  - どうしてもこのレイヤーで調整しないといけないもののみ特例でレスポンシブデザインの調整を行っても良い（基本はTemplateで行う）
  - Molecules/Atomsを使用してUI構築
  - 特定のTemplateに紐づく
- **例**: `TitleForm`, `BodyForm`, `ArticleHeader`

#### 4. Templates（テンプレート）
- **配置**: `src/features/{feature}/templates/`
- **責務**: 各ページのUI構成
- **特徴**:
  - レスポンシブ対応によりレイアウトが大きく変わる場合の切り替えを担当
  - 他のレスポンシブデザインも基本的にこのレイヤーで担当
  - 必ずしもPageと1:1対応ではない場合もある
  - Organismsを組み合わせてレイアウト構築
- **例**: `ArticleFormTemplate`, `ArticleDetailTemplate`

#### 5. Pages（ページ）
- **配置**: `src/features/{feature}/pages/`
- **責務**: ページのロジック
- **特徴**:
  - UIには一切責任を持たない
  - バックエンドとの通信等の非UIロジックを持つ
  - データフェッチング（TanStack Query）
  - 状態管理とビジネスロジック
  - Templateを呼び出してデータを渡す
- **例**: `ArticleEditPage`, `ArticleDetailPage`, `LoginPage`

## 実装時の注意点

- 基本的にサーバーサイドコンポーネントで記述し、やむを得ない場合のみ `use client` を使用してクライアントコンポーネントとしてください
- レスポンシブデザインを意識し、PCやスマホ、また縮尺を弄られた場合でも対応できるようにCSSを設定してください

## 実装時の粒度ガイドライン

### 5層で段階的に実装

AIがコンポーネントを実装する際は、以下の順序で5層のAtomic Designパターンに従って段階的に実装してください：

1. **Atomsの確認・作成** (`components/ui/`)
   - 必要な基本要素が既に存在するか確認
   - 存在しない場合は新規作成
   - 汎用的な最小単位のUIコンポーネント
   - Shadcn等のUIコンポーネントもここに配置
   - 例: `Button`, `Input`, `Label`
   - 極力サイズ等は上位レイヤーのCSSで調整できるように

2. **Moleculesの確認・作成** (`components/molecules/`)
   - Atomsを組み合わせて汎用的な複合コンポーネントを作成
   - 既存のMoleculesで対応できないか確認
   - 極力サイズ等は上位レイヤーのCSSで調整できるように
   - 例: `ArticleCard`, `UserIcon`

3. **Organismsの実装** (`features/{feature}/organisms/`)
   - 各Template特有のUI単位を作成
   - Atoms/Moleculesを組み合わせて実装
   - どうしてもこのレイヤーで調整しないといけないもののみ特例でレスポンシブデザインの調整を行っても良い（基本はTemplateで行う）
   - 例: `TitleForm`, `BodyForm`

4. **Templatesの実装** (`features/{feature}/templates/`)
   - 各ページのUI構成に責任を持つ
   - レスポンシブ対応によりレイアウトが大きく変わる場合の切り替えを担当
   - 他のレスポンシブデザインも基本的にこのレイヤーで担当
   - 必ずしもPageと1:1対応ではない場合もある
   - 例: `ArticleFormTemplate`

5. **Pagesの実装** (`features/{feature}/pages/`)
   - ページのロジックに責任を持つ
   - UIには一切責任を持たない
   - バックエンドとの通信等の非UIロジックを持つ
   - 例: `ArticleEditPage`

### 実装例

記事作成機能を実装する場合：

#### 1. Atomsの確認/作成 (`components/ui/`)
- `Button`, `Input`, `Label`, `Textarea` など
- Shadcnから利用可能なものは再利用

#### 2. Moleculesの確認/作成 (`components/molecules/`)
- `ArticleCard`, `TagChip` など
- 汎用的に使える複合コンポーネント

#### 3. Organismsの実装 (`features/article/organisms/`)
- `TitleForm`: タイトル入力フォーム
- `BodyForm`: 本文入力フォーム
- `TagInput`: タグ入力UI

#### 4. Templatesの実装 (`features/article/templates/`)
- `ArticleFormTemplate`: 記事作成フォーム全体のレイアウト
  - TitleForm, BodyForm, TagInputを配置
  - レスポンシブデザインを担当

#### 5. Pagesの実装 (`features/article/pages/`)
- `ArticleCreatePage`: 記事作成ページのロジック
  - TanStack Queryでデータフェッチング
  - フォーム送信処理
  - ArticleFormTemplateを呼び出してデータを渡す

この5層の粒度で実装することで、コンポーネントの再利用性が高まり、レビューも容易になります。
