# Auth Feature

認証に関する機能を提供するモジュールです。

## コンポーネント一覧

### Presentational Components（`components/`）

- **LoginForm.tsx** - ログインフォーム
- **SignUpForm.tsx** - サインアップフォーム

### Container Components（`containers/`）

- **LoginFormContainer.tsx** - ログインフォームのデータ管理
- **SignUpFormContainer.tsx** - サインアップフォームのデータ管理

### Custom Hooks（`hooks/`）

- **useAuth.ts** - 認証状態の管理

## 使用方法

### ログインページ

```tsx
import { LoginFormContainer } from '@/features/auth/containers/LoginFormContainer';

export default function LoginPage() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <LoginFormContainer />
    </div>
  );
}
```

### サインアップページ

```tsx
import { SignUpFormContainer } from '@/features/auth/containers/SignUpFormContainer';

export default function SignUpPage() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <SignUpFormContainer />
    </div>
  );
}
```

## 認証フロー

1. ユーザーがログイン/サインアップフォームに入力
2. ContainerがAPIを呼び出し
3. 成功時にトークンがCookieに保存される（バックエンドで管理）
4. 以降のリクエストで自動的に認証される

詳細は [認証戦略ドキュメント](/docs/authentication-strategy.md) を参照してください。
