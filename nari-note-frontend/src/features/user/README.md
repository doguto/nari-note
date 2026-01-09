# User Feature

ユーザープロフィールに関する機能を提供するモジュールです。

**重要**: Atomic Designパターンに従い、完全な機能ブロックは`organisms/`ディレクトリに配置されます。

## ディレクトリ構造

```
user/
├── organisms/              # Organisms（完全な機能ブロック）
│   └── UserProfilePage.tsx
└── types.ts               # 型定義（必要に応じて）
```

## Organisms一覧

### UserProfilePage.tsx
ユーザープロフィール表示・編集。Atoms/Moleculesを組み合わせた完全な機能。

- NameField（Molecule）を使用（編集モード時）
- EmailField（Molecule）を使用（編集モード時）
- データフェッチングとビジネスロジックを含む

## 使用方法

### プロフィールページ

```tsx
// src/app/users/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { UserProfilePage } from '@/features/user/organisms/UserProfilePage';

export default function UserProfile() {
  const params = useParams();
  const userId = Number(params.id);

  return (
    <div className="container mx-auto max-w-4xl">
      <UserProfilePage userId={userId} />
    </div>
  );
}
```

## Atomic Design構成

### 使用しているAtoms
- `FormField` - 基本フォームフィールド
- `ErrorAlert` - エラー表示

### 使用しているMolecules
- `NameField` - ユーザー名入力
- `EmailField` - メールアドレス入力（表示のみの場合もあり）

### Organisms
- `UserProfilePage` - 完全なユーザープロフィール（表示・編集機能）
