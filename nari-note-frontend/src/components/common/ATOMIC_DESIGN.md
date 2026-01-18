# Atomic Design Structure

このディレクトリは、Atomic Designパターンに基づいて構造化されています。

## ディレクトリ構造

```
common/
├── atoms/        # 最小単位のコンポーネント
├── molecules/    # Atomsを組み合わせたコンポーネント
└── [organisms]   # featuresディレクトリで定義
```

## Atoms（原子）

最小単位の再利用可能なUIコンポーネント。これ以上分割できない基本要素。

### 例
- **FormField** - ラベル + 入力フィールドのセット
- **ErrorAlert** - エラーメッセージ表示（インラインエラー用）
- **ErrorMessage** - エラーメッセージ表示（再試行ボタン付き）
- **FormTitle** - フォームタイトル
- **TagChip** - タグチップ（削除ボタン付き）
- **LoadingSpinner** - ローディングスピナー
- **EmptyState** - 空状態表示
- **LikeButton** - いいねボタン
- **FollowButton** - フォローボタン
- **FollowStats** - フォロワー統計表示

## Molecules（分子）

複数のAtomsを組み合わせて、特定の機能を持つコンポーネント。

### 例
- **EmailField** - メールアドレス入力（FormFieldを使用）
- **PasswordField** - パスワード入力（FormFieldを使用）
- **NameField** - ユーザー名入力（FormFieldを使用）
- **TagInput** - タグ入力（Input + Button + TagChipを使用）
- **CharacterCounter** - 文字数カウンター
- **ArticleCard** - 記事カード（ホーム画面などで使用）
- **CommentItem** - コメント表示項目
- **UserListItem** - ユーザーリスト項目

## Organisms（生体）

Atoms/Moleculesを組み合わせた、完全な機能を持つコンポーネント。
featuresディレクトリ内で定義されます。

### 例
- **LoginPage** - ログインページ
- **SignUpPage** - 新規登録ページ
- **ArticleFormPage** - 記事作成・編集ページ
- **ArticleDetailPage** - 記事詳細ページ
- **UserProfilePage** - ユーザープロフィールページ

## 使用方法

### Atomsのインポート

```tsx
import { FormField, ErrorAlert, FormTitle, LoadingSpinner, EmptyState, ErrorMessage } from '@/components/common/atoms';
```

### Moleculesのインポート

```tsx
import { EmailField, PasswordField, TagInput, ArticleCard } from '@/components/common/molecules';
```

## メリット

1. **再利用性** - 小さなコンポーネントを組み合わせて複雑なUIを構築
2. **保守性** - 各コンポーネントが独立しているため、変更の影響範囲が明確
3. **テスト容易性** - 小さな単位でテストが可能
4. **一貫性** - 同じコンポーネントを使用することでUIの一貫性を保つ
5. **レビュー容易性** - ファイルが細かく分かれているため、レビューがしやすい

## ガイドライン

- **Atoms** は shadcn UIコンポーネントまたは基本的なHTML要素で構築
- **Molecules** は Atoms を組み合わせて特定の機能を実装
- **Organisms** は features ディレクトリ内で Atoms/Molecules を組み合わせて完全な機能を実装
- 各コンポーネントは単一責任の原則に従う
- propsの型定義を明確にする

## コンポーネント分類の基準

### Atomsに分類すべきもの
- これ以上分割できない最小単位のUI要素
- 他のコンポーネントから独立して機能する
- 状態管理やビジネスロジックを持たない
- 例: ボタン、入力フィールド、ラベル、スピナー

### Moleculesに分類すべきもの
- 複数のAtomsを組み合わせた機能的なコンポーネント
- 特定の機能を持つが、ページ全体の機能ではない
- ビジネスロジックは最小限
- 例: フォームフィールド（ラベル+入力+エラー）、カード、リスト項目

### Organismsに分類すべきもの
- ページの主要な機能ブロック
- ビジネスロジックとデータ取得を含む
- Atoms/Moleculesを組み合わせて完全な機能を実現
- 例: ログインフォーム、記事詳細、ユーザープロフィール

