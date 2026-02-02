# UserAvatar Component

ユーザーアバターを表示するAtomコンポーネントです。

## 概要

- プロフィール画像がある場合は画像を表示
- プロフィール画像がない場合はユーザー名の最初の文字を表示
- 画像読み込みエラー時は自動的にフォールバック表示に切り替え
- 4つのサイズオプション: sm, md, lg, xl

## 使用例

### 基本的な使用方法（画像なし）

```tsx
import { UserAvatar } from '@/components/ui';

<UserAvatar username="田中太郎" />
```

結果: ブランドカラーの円形アバターに「田」の文字が表示されます。

### プロフィール画像付き

```tsx
<UserAvatar 
  username="田中太郎" 
  profileImage="https://example.com/avatar.jpg" 
/>
```

結果: プロフィール画像が円形で表示されます。

### サイズ指定

```tsx
// 小サイズ（8x8 = 32px）- ArticleCardなどで使用
<UserAvatar username="田中太郎" size="sm" />

// 中サイズ（12x12 = 48px）- デフォルト、UserListItemで使用
<UserAvatar username="田中太郎" size="md" />

// 大サイズ（20x20 = 80px）
<UserAvatar username="田中太郎" size="lg" />

// 特大サイズ（24x24 = 96px）- UserProfileTemplateで使用
<UserAvatar username="田中太郎" size="xl" />
```

## Props

| Prop | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| username | string | ✅ | - | ユーザー名（画像がない場合、最初の文字を表示） |
| profileImage | string | ❌ | undefined | プロフィール画像のURL |
| size | 'sm' \| 'md' \| 'lg' \| 'xl' | ❌ | 'md' | アバターのサイズ |
| className | string | ❌ | '' | 追加のCSSクラス |

## 技術的な詳細

### 画像の最適化

- Next.jsの`Image`コンポーネントを使用して自動最適化
- サイズに応じた適切な`sizes`属性を設定
- `fill`レイアウトで親要素のサイズに合わせて表示

### エラーハンドリング

- 画像読み込みエラー時は`onError`イベントで状態を更新
- 自動的にフォールバック表示（ユーザー名の最初の文字）に切り替え

### アクセシビリティ

- 画像には適切な`alt`属性を設定
- フォールバック表示には`aria-label`属性を設定

## 使用箇所

現在、以下のコンポーネントで使用されています：

1. **UserListItem** (Molecule)
   - サイズ: md
   - フォロワー・フォロー一覧で使用

2. **UserProfileTemplate** (Template)
   - サイズ: xl
   - ユーザープロフィールページのヘッダーで使用

3. **ArticleCard** (Molecule)
   - サイズ: sm
   - 記事一覧カードで著者情報を表示

## 今後の拡張予定

- オンライン/オフラインステータスインジケーター
- バッジ表示（認証済み、管理者など）
- ホバー時のツールチップ
