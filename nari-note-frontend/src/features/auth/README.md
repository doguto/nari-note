# Auth Feature

認証に関する機能を提供するモジュールです。

## ディレクトリ構造

```
auth/
├── pages/              # Pageコンポーネント（ロジック担当）
│   ├── LoginPage.tsx
│   └── SignUpPage.tsx
└── templates/          # Templateコンポーネント（UI担当）
    ├── LoginTemplate.tsx
    └── SignUpTemplate.tsx
```

## Pages一覧

### LoginPage.tsx
ログインページのロジックに責任を持つコンポーネント。UIには一切責任を持たない。

- バックエンドとの通信（`useSignIn`）
- リダイレクト処理（セキュリティ検証含む）
- フォームバリデーション

### SignUpPage.tsx
サインアップページのロジックに責任を持つコンポーネント。UIには一切責任を持たない。

- バックエンドとの通信（`useSignUp`）
- フォームバリデーション

## 使用方法

### ログインページ

```tsx
// src/app/(with-layout)/(auth)/login/page.tsx
import { LoginPage } from '@/features/auth/pages/LoginPage';

export default function LoginPageRoute() {
  return <LoginPage />;
}
```

### サインアップページ

```tsx
// src/app/(with-layout)/(auth)/signup/page.tsx
import { SignUpPage } from '@/features/auth/pages/SignUpPage';

export default function SignUpPageRoute() {
  return <SignUpPage />;
}
```

## Atomic Design構成

### 使用しているAtoms
- `FormField` - 基本フォームフィールド
- `ErrorAlert` - エラー表示
- `FormTitle` - フォームタイトル

### 使用しているMolecules
- `EmailField` - メールアドレス入力（FormFieldを使用）
- `PasswordField` - パスワード入力（FormFieldを使用）
- `NameField` - ユーザー名入力（FormFieldを使用）
- `AuthPageLayout` - 認証ページ共通レイアウト

### Templates
- `LoginTemplate` - ログインフォームUI
- `SignUpTemplate` - 新規登録フォームUI

## 認証フロー

1. ユーザーがログイン/サインアップフォームに入力
2. PageコンポーネントがAPIを呼び出し
3. 成功時にトークンがCookieに保存される（バックエンドで管理）
4. 以降のリクエストで自動的に認証される

詳細は [認証戦略ドキュメント](/docs/authentication-strategy.md) を参照してください。
