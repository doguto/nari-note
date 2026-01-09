# Auth Feature

認証に関する機能を提供するモジュールです。

**重要**: Atomic Designパターンに従い、完全な機能ブロックは`organisms/`ディレクトリに配置されます。

## ディレクトリ構造

```
auth/
├── organisms/          # Organisms（完全な機能ブロック）
│   ├── LoginPage.tsx
│   └── SignUpPage.tsx
└── types.ts           # 型定義（必要に応じて）
```

## Organisms一覧

### LoginPage.tsx
ログインフォーム。Atoms/Moleculesを組み合わせた完全な機能。

- EmailField（Molecule）を使用
- PasswordField（Molecule）を使用
- ErrorAlert（Atom）を使用
- データフェッチングとビジネスロジックを含む

### SignUpPage.tsx
新規登録フォーム。Atoms/Moleculesを組み合わせた完全な機能。

- NameField（Molecule）を使用
- EmailField（Molecule）を使用
- PasswordField（Molecule）を使用
- ErrorAlert（Atom）を使用
- データフェッチングとビジネスロジックを含む

## 使用方法

### ログインページ

```tsx
// src/app/(auth)/login/page.tsx
import { LoginPage } from '@/features/auth/organisms/LoginPage';

export default function Login() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="max-w-md w-full">
        <LoginPage />
      </div>
    </div>
  );
}
```

### サインアップページ

```tsx
// src/app/(auth)/signup/page.tsx
import { SignUpPage } from '@/features/auth/organisms/SignUpPage';

export default function SignUp() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="max-w-md w-full">
        <SignUpPage />
      </div>
    </div>
  );
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

### Organisms
- `LoginPage` - 完全なログインフォーム（Molecules + ビジネスロジック）
- `SignUpPage` - 完全な新規登録フォーム（Molecules + ビジネスロジック）

## 認証フロー

1. ユーザーがログイン/サインアップフォームに入力
2. ContainerがAPIを呼び出し
3. 成功時にトークンがCookieに保存される（バックエンドで管理）
4. 以降のリクエストで自動的に認証される

詳細は [認証戦略ドキュメント](/docs/authentication-strategy.md) を参照してください。
