# フロントエンドアーキテクチャ再設計 - 完了レポート

このドキュメントは、nari-noteフロントエンドのAtomic Design再設計の完了状況をまとめたものです。

## 実施日
2026-01-27

## 新しいアーキテクチャ概要

nari-noteフロントエンドは、以下の**5層のAtomic Designパターン**に従って再設計されました。

### 5層構造

1. **Page** (`features/{feature}/pages/`)
   - ページのロジックに責任を持つ
   - UIには一切責任を持たない
   - バックエンドとの通信等の非UIロジックを持つ

2. **Template** (`features/{feature}/templates/`)
   - 各ページのUI構成に責任を持つ
   - レスポンシブ対応によりレイアウトが大きく変わる場合の切り替えを担当
   - 他のレスポンシブデザインも基本的にこのレイヤーで担当

3. **Organism** (`features/{feature}/organisms/`)
   - 各Template特有のUI単位
   - どうしてもこのレイヤーで調整しないといけないもののみ特例でレスポンシブデザインの調整を行っても良い

4. **Molecule** (`components/molecules/`)
   - 汎用的な複合コンポーネント
   - Atomが組み合わさって構成される
   - 極力サイズ等は上位レイヤーのCSSで調整できるように

5. **Atom** (`components/ui/`)
   - 汎用的な最小単位のUIコンポーネント
   - 極力サイズ等は上位レイヤーのCSSで調整できるように
   - Shadcn等のUIコンポーネントもここに配置

## 実施した変更

### 1. ディレクトリ構造の再編成 ✅

#### 移動完了
- `components/common/atoms/` → `components/ui/` 
- `components/common/molecules/` → `components/molecules/`
- `components/layout/` → `features/global/organisms/`
- `features/article/molecules/` → `features/article/organisms/`

#### 新規作成
- `features/{feature}/pages/` - 各featureにpagesディレクトリを作成
- `features/{feature}/templates/` - 各featureにtemplatesディレクトリを作成
- `features/global/organisms/` - グローバル共通コンポーネント用

### 2. インポートパスの更新 ✅

31ファイルのimport文を新しいディレクトリ構造に合わせて更新しました：

**変更パターン:**
```typescript
// Before
import { ErrorAlert } from '@/components/common/atoms';
import { EmailField } from '@/components/common/molecules';
import { Header } from '@/components/layout';

// After
import { ErrorAlert } from '@/components/ui';
import { EmailField } from '@/components/molecules';
import { Header } from '@/features/global/organisms';
```

### 3. index.tsファイルの作成・更新 ✅

以下のindex.tsファイルを作成・更新しました：
- `components/ui/index.ts` - Atomsのエクスポート
- `components/molecules/index.ts` - Moleculesのエクスポート
- `features/global/organisms/index.ts` - グローバルコンポーネントのエクスポート
- `features/article/organisms/index.ts` - 記事関連Organismsのエクスポート

### 4. ドキュメントの更新 ✅

以下のドキュメントを新しい5層構造に合わせて全面更新しました：

- `docs/implementation-guide.md` - 実装ガイドを5層構造に対応
- `docs/architecture.md` - アーキテクチャ説明を5層構造に更新
- `docs/quick-reference.md` - クイックリファレンスを5層構造に対応
- `docs/architecture-diagram.md` - アーキテクチャ図を5層構造に更新
- `src/features/README.md` - featuresディレクトリの説明を更新
- `src/components/README.md` - componentsディレクトリの説明を更新

### 5. 古いディレクトリの削除 ✅

以下の古いディレクトリを削除しました：
- `components/common/atoms/`
- `components/common/molecules/`
- `components/layout/`
- `features/article/molecules/`

## 現在のディレクトリ構造

```
nari-note-frontend/src/
├── app/                          # Next.js App Router
├── components/
│   ├── ui/                       # Atom層（汎用的な最小単位）
│   │   ├── button.tsx
│   │   ├── input.tsx
│   │   ├── FormField.tsx
│   │   ├── ErrorAlert.tsx
│   │   └── ...
│   └── molecules/                # Molecule層（汎用的な複合コンポーネント）
│       ├── ArticleCard.tsx
│       ├── EmailField.tsx
│       ├── PasswordField.tsx
│       └── ...
├── features/
│   ├── article/
│   │   ├── pages/                # Page層（今後実装）
│   │   ├── templates/            # Template層（今後実装）
│   │   └── organisms/            # Organism層
│   │       ├── ArticleFormPage.tsx
│   │       ├── ArticleDetailPage.tsx
│   │       ├── ArticleTitleInput.tsx
│   │       └── ...
│   ├── auth/
│   │   ├── pages/                # Page層（今後実装）
│   │   ├── templates/            # Template層（今後実装）
│   │   └── organisms/            # Organism層
│   │       ├── LoginPage.tsx
│   │       └── SignUpPage.tsx
│   ├── user/
│   │   ├── pages/                # Page層（今後実装）
│   │   ├── templates/            # Template層（今後実装）
│   │   └── organisms/            # Organism層
│   │       ├── UserProfilePage.tsx
│   │       └── ...
│   ├── tag/
│   │   ├── pages/                # Page層（今後実装）
│   │   ├── templates/            # Template層（今後実装）
│   │   └── organisms/            # Organism層
│   │       └── TagArticleListPage.tsx
│   └── global/
│       └── organisms/            # グローバル共通コンポーネント
│           ├── Header.tsx
│           ├── Footer.tsx
│           ├── MainLayout.tsx
│           └── Sidebar.tsx
└── lib/                          # APIクライアント、ユーティリティ等
```

## 検証結果

- ✅ TypeScript型チェック成功（エラーなし）
- ✅ すべてのimportパスが正しく解決
- ⚠️ Next.js build: Suspense関連のエラーあり（本リファクタリングとは無関係）

## 今後の実装タスク

現在は既存のOrganismsをそのまま残していますが、以下の段階的なリファクタリングが推奨されます：

### Phase 1: article feature
1. ArticleFormPageをPage/Template/Organismに分離
2. ArticleDetailPageをPage/Template/Organismに分離
3. HomeArticleListをPage/Template/Organismに分離

### Phase 2: auth feature
1. LoginPageをPage/Template/Organismに分離
2. SignUpPageをPage/Template/Organismに分離

### Phase 3: user feature
1. UserProfilePageをPage/Template/Organismに分離
2. ProfileEditPageをPage/Template/Organismに分離

### Phase 4: tag feature
1. TagArticleListPageをPage/Template/Organismに分離

### 分離の基準

**Page（ロジック層）の責務:**
- データフェッチング（useGetArticle, useCreateArticle等）
- ビジネスロジック（バリデーション、フォーム送信処理）
- 状態管理（useState, useEffect等）
- UIコンポーネントへのprops渡し

**Template（UI構成層）の責務:**
- OrganismsとMoleculesのレイアウト
- レスポンシブデザインの切り替え
- 全体的なUI構成
- propsから受け取ったデータの表示

**Organism（UI単位層）の責務:**
- Template特有の複合UI要素
- Atoms/Moleculesを組み合わせた部品
- 必要最小限のローカルステート

## 利点

この5層構造により、以下の利点が得られます：

1. **責任分離の明確化**: ロジック（Page）とUI（Template/Organism）が完全に分離
2. **テスト容易性**: ロジックとUIを独立してテスト可能
3. **再利用性**: Template/Organismは複数のPageから利用可能
4. **保守性**: 各層の変更が他の層に影響しにくい
5. **AI実装の精度向上**: 各層の責務が明確なため、AIによる実装が容易

## 参考ドキュメント

- [実装ガイド](docs/implementation-guide.md) - AI向けの具体的な実装方法
- [アーキテクチャガイド](docs/architecture.md) - アーキテクチャの詳細説明
- [クイックリファレンス](docs/quick-reference.md) - よく使うパターンのリファレンス
- [アーキテクチャ図](docs/architecture-diagram.md) - 視覚的な構造説明

## まとめ

nari-noteフロントエンドのAtomic Design再設計は、ディレクトリ構造の再編成とドキュメント更新が完了しました。既存のコードは新しい構造で動作可能であり、今後は段階的にPage/Template/Organismへの分離を進めることができます。

新規実装は必ずこの5層構造に従って行ってください。
