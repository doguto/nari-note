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
- **ErrorAlert** - エラーメッセージ表示
- **FormTitle** - フォームタイトル
- **TagChip** - タグチップ（削除ボタン付き）

## Molecules（分子）

複数のAtomsを組み合わせて、特定の機能を持つコンポーネント。

### 例
- **EmailField** - メールアドレス入力（FormFieldを使用）
- **PasswordField** - パスワード入力（FormFieldを使用）
- **NameField** - ユーザー名入力（FormFieldを使用）
- **TagInput** - タグ入力（Input + Button + TagChipを使用）
- **CharacterCounter** - 文字数カウンター

## Organisms（生体）

Atoms/Moleculesを組み合わせた、完全な機能を持つコンポーネント。
featuresディレクトリ内で定義されます。

### 例
- **LoginForm** - ログインフォーム
- **SignUpForm** - 新規登録フォーム
- **ArticleForm** - 記事作成・編集フォーム

## 使用方法

### Atomsのインポート

```tsx
import { FormField, ErrorAlert, FormTitle } from '@/components/common/atoms';
```

### Moleculesのインポート

```tsx
import { EmailField, PasswordField, TagInput } from '@/components/common/molecules';
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
