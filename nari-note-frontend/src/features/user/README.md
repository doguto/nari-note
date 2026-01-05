# User Feature

ユーザープロフィールに関する機能を提供するモジュールです。

## コンポーネント一覧

### Presentational Components（`components/`）

- **UserProfile.tsx** - ユーザープロフィール表示
- **UserProfileForm.tsx** - プロフィール編集フォーム

### Container Components（`containers/`）

- **UserProfileContainer.tsx** - プロフィール表示のデータ管理
- **UserProfileFormContainer.tsx** - プロフィール編集のデータ管理

### Custom Hooks（`hooks/`）

- **useUserProfile.ts** - プロフィール管理のロジック

## 使用方法

### プロフィールページ

```tsx
import { UserProfileContainer } from '@/features/user/containers/UserProfileContainer';

export default function UserProfilePage({ params }: { params: { id: string } }) {
  return <UserProfileContainer userId={Number(params.id)} />;
}
```

### プロフィール編集ページ

```tsx
import { UserProfileFormContainer } from '@/features/user/containers/UserProfileFormContainer';

export default function EditProfilePage() {
  return <UserProfileFormContainer />;
}
```
