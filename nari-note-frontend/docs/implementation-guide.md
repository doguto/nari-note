# フロントエンド実装ガイド（AI エージェント向け）

このドキュメントは、AIエージェントがnari-note-frontendのコードを生成する際の具体的なガイドラインです。

**重要**: nari-noteでは5層のAtomic Designパターン（Page → Template → Organism → Molecule → Atom）を採用しています。

## 目次

1. [コード生成の基本ルール](#コード生成の基本ルール)
2. [Atomic Design階層](#atomic-design階層)
3. [ディレクトリ配置ルール](#ディレクトリ配置ルール)
4. [命名規則](#命名規則)
5. [コンポーネント生成パターン](#コンポーネント生成パターン)
6. [よくあるパターン](#よくあるパターン)

## コード生成の基本ルール

### 1. 5層のAtomic Designパターンを必ず使用

コンポーネントを小さな単位で階層的に実装してください。

#### 実装の順序

1. **Atomsの確認・作成** (`components/ui/`)
   - 必要な基本要素が既に存在するか確認
   - 存在しない場合は新規作成
   - 汎用的な最小単位のUIコンポーネント
   - Shadcn等のUIコンポーネントもここに配置
   - 例: Button, Input, Label
   - 極力サイズ等は上位レイヤーのCSSで調整できるように

2. **Moleculesの確認・作成** (`components/molecules/`)
   - Atomsを組み合わせて汎用的な複合コンポーネントを作成
   - 既存のMoleculesで対応できないか確認
   - 極力サイズ等は上位レイヤーのCSSで調整できるように
   - 例: ArticleCard, UserIcon

3. **Organismsの実装** (`features/{feature}/organisms/`)
   - 各Template特有のUI単位を作成
   - Atoms/Moleculesを組み合わせて実装
   - どうしてもこのレイヤーで調整しないといけないもののみ特例でレスポンシブデザインの調整を行っても良い（基本はTemplateで行う）
   - 例: TitleForm, BodyForm

4. **Templatesの実装** (`features/{feature}/templates/`)
   - 各ページのUI構成に責任を持つ
   - レスポンシブ対応によりレイアウトが大きく変わる場合の切り替えを担当
   - 他のレスポンシブデザインも基本的にこのレイヤーで担当
   - 必ずしもPageと1:1対応ではない場合もある
   - 例: ArticleFormTemplate

5. **Pagesの実装** (`features/{feature}/pages/`)
   - ページのロジックに責任を持つ
   - UIには一切責任を持たない
   - バックエンドとの通信等の非UIロジックを持つ
   - 例: ArticleEditPage

```tsx
// ❌ 悪い例: すべてを1つのコンポーネントに詰め込む
export function LoginForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  return (
    <form>
      <div>
        <label>メールアドレス</label>
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
      </div>
      <div>
        <label>パスワード</label>
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      </div>
      <button type="submit">ログイン</button>
    </form>
  );
}

// ✅ 良い例: 5層のAtomic Designで分割

// Atom: Button (components/ui/button.tsx)
export function Button({ children, type, onClick }: ButtonProps) {
  return (
    <button type={type} onClick={onClick} className="btn-primary">
      {children}
    </button>
  );
}

// Atom: Input (components/ui/input.tsx)
export function Input({ id, type, value, onChange }: InputProps) {
  return (
    <input
      id={id}
      type={type}
      value={value}
      onChange={onChange}
      className="input"
    />
  );
}

// Molecule: EmailField (components/molecules/EmailField.tsx)
export function EmailField({ value, onChange, error }: EmailFieldProps) {
  return (
    <div>
      <Label htmlFor="email">メールアドレス</Label>
      <Input id="email" type="email" value={value} onChange={onChange} />
      {error && <span className="error">{error}</span>}
    </div>
  );
}

// Organism: LoginForm (features/auth/organisms/LoginForm.tsx)
export function LoginForm({ email, password, onEmailChange, onPasswordChange, onSubmit }: LoginFormProps) {
  return (
    <form onSubmit={onSubmit}>
      <EmailField value={email} onChange={onEmailChange} />
      <PasswordField value={password} onChange={onPasswordChange} />
      <Button type="submit">ログイン</Button>
    </form>
  );
}

// Template: AuthFormTemplate (features/auth/templates/AuthFormTemplate.tsx)
export function AuthFormTemplate({ title, form, footer }: AuthFormTemplateProps) {
  return (
    <div className="max-w-md mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">{title}</h1>
      <div className="space-y-4">{form}</div>
      {footer && <div className="mt-4">{footer}</div>}
    </div>
  );
}

// Page: LoginPage (features/auth/pages/LoginPage.tsx)
export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const router = useRouter();
  
  const login = useLogin({
    onSuccess: () => {
      router.push('/');
    },
  });
  
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login.mutate({ email, password });
  };
  
  return (
    <AuthFormTemplate
      title="ログイン"
      form={
        <LoginForm
          email={email}
          password={password}
          onEmailChange={setEmail}
          onPasswordChange={setPassword}
          onSubmit={handleSubmit}
        />
      }
    />
  );
}
```

### 2. Page/Template/Organismパターン

新しいAtomic Designでは、**Page**がロジック、**Template**がUI構成、**Organism**がTemplate特有のUI単位という3層構造になっています。

```tsx
// Page（ロジックのみ、UIなし）
export function ArticleDetailPage({ articleId }: { articleId: number }) {
  const { data, isLoading, error } = useGetArticle({ id: articleId });
  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="エラー" />;
  if (!data) return null;
  
  return <ArticleDetailTemplate article={data} />;
}

// Template（UI構成とレスポンシブ対応）
export function ArticleDetailTemplate({ article }: { article: GetArticleResponse }) {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <ArticleHeader article={article} />
      <ArticleBody body={article.body} />
      <ArticleTags tags={article.tags} />
    </div>
  );
}

// Organism（Template特有のUI単位）
export function ArticleHeader({ article }: { article: GetArticleResponse }) {
  return (
    <header>
      <h1 className="text-3xl font-bold">{article.title}</h1>
      <div className="text-gray-600">
        <span>著者: {article.authorName}</span>
        <span>いいね: {article.likeCount}</span>
      </div>
    </header>
  );
}
```

### 3. 型定義を明確にする

すべてのpropsに型定義を追加してください。

```tsx
// ✅ 良い例
interface ArticleCardProps {
  article: GetArticleResponse;
  onLike?: () => void;
  showAuthor?: boolean;
}

export function ArticleCard({ article, onLike, showAuthor = true }: ArticleCardProps) {
  // ...
}
```

### 4. 共通コンポーネントを活用する

Loading、ErrorMessage、EmptyStateなどの共通コンポーネントを使用してください。

```tsx
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { EmptyState } from '@/components/common/EmptyState';
```

## Atomic Design階層

### 階層の説明

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

### 各階層の詳細

#### 1. Atoms（`components/ui/`）
- **責務**: 汎用的な最小単位のUIコンポーネント
- **特徴**:
  - Shadcn等のUIコンポーネントもここに配置
  - 極力サイズ等は上位レイヤーのCSSで調整できるように
  - 他のコンポーネントに依存しない
- **例**: `Button`, `Input`, `Link`, `Label`

#### 2. Molecules（`components/molecules/`）
- **責務**: 汎用的な複合コンポーネント
- **特徴**:
  - Atomsが組み合わさって構成される
  - 極力サイズ等は上位レイヤーのCSSで調整できるように
  - 複数の機能で再利用可能
- **例**: `ArticleCard`, `UserIcon`, `SearchBar`

#### 3. Organisms（`features/{feature}/organisms/`）
- **責務**: 各Template特有のUI単位
- **特徴**:
  - どうしてもこのレイヤーで調整しないといけないもののみ特例でレスポンシブデザインの調整を行っても良い（基本はTemplateで行う）
  - Molecules/Atomsを使用してUI構築
  - 特定のTemplateに紐づく
- **例**: `TitleForm`, `BodyForm`, `ArticleHeader`

#### 4. Templates（`features/{feature}/templates/`）
- **責務**: 各ページのUI構成
- **特徴**:
  - レスポンシブ対応によりレイアウトが大きく変わる場合の切り替えを担当
  - 他のレスポンシブデザインも基本的にこのレイヤーで担当
  - 必ずしもPageと1:1対応ではない場合もある
  - Organismsを組み合わせてレイアウト構築
- **例**: `ArticleFormTemplate`, `ArticleDetailTemplate`

#### 5. Pages（`features/{feature}/pages/`）
- **責務**: ページのロジック
- **特徴**:
  - UIには一切責任を持たない
  - バックエンドとの通信等の非UIロジックを持つ
  - データフェッチング（TanStack Query）
  - 状態管理とビジネスロジック
  - Templateを呼び出してデータを渡す
- **例**: `ArticleEditPage`, `ArticleDetailPage`, `LoginPage`

## ディレクトリ配置ルール

### Atomic Design構造

```
src/
├── components/
│   ├── ui/                     # Atoms: 汎用的な最小単位のUIコンポーネント
│   │   ├── button.tsx
│   │   ├── input.tsx
│   │   ├── label.tsx
│   │   ├── link.tsx
│   │   └── index.ts
│   └── molecules/              # Molecules: 汎用的な複合コンポーネント
│       ├── ArticleCard.tsx
│       ├── UserIcon.tsx
│       ├── SearchBar.tsx
│       └── index.ts
├── features/
│   ├── article/                # 記事機能
│   │   ├── pages/              # Pages: ロジック層
│   │   │   ├── ArticleDetailPage.tsx
│   │   │   ├── ArticleEditPage.tsx
│   │   │   └── ArticleCreatePage.tsx
│   │   ├── templates/          # Templates: UI構成層
│   │   │   ├── ArticleDetailTemplate.tsx
│   │   │   └── ArticleFormTemplate.tsx
│   │   ├── organisms/          # Organisms: Template特有のUI単位
│   │   │   ├── ArticleHeader.tsx
│   │   │   ├── ArticleBody.tsx
│   │   │   ├── TitleForm.tsx
│   │   │   └── BodyForm.tsx
│   │   └── types.ts
│   ├── auth/                   # 認証機能
│   │   ├── pages/
│   │   │   ├── LoginPage.tsx
│   │   │   └── SignUpPage.tsx
│   │   ├── templates/
│   │   │   └── AuthFormTemplate.tsx
│   │   ├── organisms/
│   │   │   ├── LoginForm.tsx
│   │   │   └── SignUpForm.tsx
│   │   └── types.ts
│   ├── global/                 # グローバル機能（Header等）
│   │   └── organisms/
│   │       ├── Header.tsx
│   │       └── Footer.tsx
│   ├── user/                   # ユーザー機能
│   │   ├── pages/
│   │   ├── templates/
│   │   └── organisms/
│   └── tag/                    # タグ機能
│       ├── pages/
│       ├── templates/
│       └── organisms/
└── app/                        # Next.js App Router
    └── ...
```

### 配置ルール

| 何を作る？ | どこに配置？ | 例 |
|-----------|------------|-----|
| Atoms（汎用UI） | `src/components/ui/` | `button.tsx`, `input.tsx`, `label.tsx` |
| Molecules（汎用複合） | `src/components/molecules/` | `ArticleCard.tsx`, `UserIcon.tsx` |
| Organisms（Template特有UI） | `src/features/{feature}/organisms/` | `TitleForm.tsx`, `ArticleHeader.tsx` |
| Templates（UI構成） | `src/features/{feature}/templates/` | `ArticleFormTemplate.tsx`, `AuthFormTemplate.tsx` |
| Pages（ロジック） | `src/features/{feature}/pages/` | `ArticleEditPage.tsx`, `LoginPage.tsx` |
| グローバルコンポーネント | `src/features/global/organisms/` | `Header.tsx`, `Footer.tsx` |
| App Routerページ | `src/app/{route}/` | `page.tsx` |
| ユーティリティ関数 | `src/lib/utils/` | `format.ts`, `validation.ts` |
| カスタムフック（共通） | `src/lib/hooks/` | `useDebounce.ts` |

## 命名規則

### コンポーネントファイル

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| Page Component | `{Feature}{Action}Page.tsx` | `ArticleEditPage.tsx`, `LoginPage.tsx` |
| Template Component | `{Feature}{Type}Template.tsx` | `ArticleFormTemplate.tsx`, `AuthFormTemplate.tsx` |
| Organism Component | `{ComponentName}.tsx` | `TitleForm.tsx`, `ArticleHeader.tsx` |
| Molecule Component | `{ComponentName}.tsx` | `ArticleCard.tsx`, `UserIcon.tsx` |
| Atom Component | `{component-name}.tsx` (kebab-case) | `button.tsx`, `input.tsx` |
| App Router Page | `page.tsx` | `page.tsx` |
| App Router Layout | `layout.tsx` | `layout.tsx` |

### 非コンポーネントファイル

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| カスタムフック | `use{HookName}.ts` | `useArticleForm.ts` |
| ユーティリティ | `{utilName}.ts` | `format.ts` |
| 型定義 | `types.ts` | `types.ts` |

### 変数・関数

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| コンポーネント | PascalCase | `ArticleCard` |
| 関数 | camelCase | `handleSubmit` |
| 定数 | UPPER_SNAKE_CASE | `API_BASE_URL` |
| カスタムフック | camelCase (use始まり) | `useArticleForm` |

## コンポーネント生成パターン

### パターン1: Atomの作成

**要件:** ボタンコンポーネントを作成（Shadcnベース）

**生成するファイル:**
1. `src/components/ui/button.tsx`

**実装例:**
```tsx
// src/components/ui/button.tsx
import * as React from 'react';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline';
  size?: 'sm' | 'md' | 'lg';
}

export function Button({ 
  children, 
  variant = 'primary', 
  size = 'md',
  className,
  ...props 
}: ButtonProps) {
  const baseStyles = 'rounded font-medium transition-colors';
  const variantStyles = {
    primary: 'bg-[#88b04b] text-white hover:bg-[#769939]',
    secondary: 'bg-[#2d3e1f] text-white hover:bg-[#1f2a15]',
    outline: 'border border-gray-300 hover:bg-gray-50',
  };
  const sizeStyles = {
    sm: 'px-3 py-1 text-sm',
    md: 'px-4 py-2',
    lg: 'px-6 py-3 text-lg',
  };

  return (
    <button
      className={`${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
```

### パターン2: Moleculeの作成

**要件:** 記事カードコンポーネントを作成

**生成するファイル:**
1. `src/components/molecules/ArticleCard.tsx`

**実装例:**
```tsx
// src/components/molecules/ArticleCard.tsx
import { Link } from '@/components/ui/link';
import { Button } from '@/components/ui/button';

interface ArticleCardProps {
  id: number;
  title: string;
  excerpt: string;
  authorName: string;
  likeCount: number;
  tags: string[];
}

export function ArticleCard({ 
  id, 
  title, 
  excerpt, 
  authorName, 
  likeCount, 
  tags 
}: ArticleCardProps) {
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <Link href={`/articles/${id}`}>
        <h3 className="text-xl font-bold text-[#2d3e1f] hover:text-[#88b04b]">
          {title}
        </h3>
      </Link>
      <p className="mt-2 text-gray-600">{excerpt}</p>
      <div className="mt-4 flex items-center justify-between">
        <span className="text-sm text-gray-500">by {authorName}</span>
        <span className="text-sm text-gray-500">❤️ {likeCount}</span>
      </div>
      {tags.length > 0 && (
        <div className="mt-4 flex gap-2">
          {tags.map((tag, i) => (
            <span key={i} className="px-2 py-1 bg-green-100 text-green-800 rounded text-sm">
              {tag}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
```

### パターン3: Organismの作成

**要件:** ログインフォームのUI（Organism）を作成

**生成するファイル:**
1. `src/features/auth/organisms/LoginForm.tsx`

**実装例:**
```tsx
// src/features/auth/organisms/LoginForm.tsx
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface LoginFormProps {
  email: string;
  password: string;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isLoading?: boolean;
  errors?: {
    email?: string;
    password?: string;
  };
}

export function LoginForm({ 
  email, 
  password, 
  onEmailChange, 
  onPasswordChange, 
  onSubmit,
  isLoading = false,
  errors = {},
}: LoginFormProps) {
  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div>
        <Label htmlFor="email">メールアドレス</Label>
        <Input
          id="email"
          type="email"
          value={email}
          onChange={(e) => onEmailChange(e.target.value)}
          placeholder="example@example.com"
        />
        {errors.email && (
          <p className="mt-1 text-sm text-red-500">{errors.email}</p>
        )}
      </div>
      
      <div>
        <Label htmlFor="password">パスワード</Label>
        <Input
          id="password"
          type="password"
          value={password}
          onChange={(e) => onPasswordChange(e.target.value)}
          placeholder="••••••••"
        />
        {errors.password && (
          <p className="mt-1 text-sm text-red-500">{errors.password}</p>
        )}
      </div>
      
      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'ログイン中...' : 'ログイン'}
      </Button>
    </form>
  );
}
```

### パターン4: Templateの作成

**要件:** 認証フォームのテンプレート（Template）を作成

**生成するファイル:**
1. `src/features/auth/templates/AuthFormTemplate.tsx`

**実装例:**
```tsx
// src/features/auth/templates/AuthFormTemplate.tsx
interface AuthFormTemplateProps {
  title: string;
  form: React.ReactNode;
  footer?: React.ReactNode;
  error?: string;
}

export function AuthFormTemplate({ 
  title, 
  form, 
  footer,
  error,
}: AuthFormTemplateProps) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-[#f5f3e8]">
      <div className="w-full max-w-md">
        <div className="bg-white rounded-lg shadow-lg p-8">
          <h1 className="text-2xl font-bold text-[#2d3e1f] mb-6 text-center">
            {title}
          </h1>
          
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded">
              <p className="text-sm text-red-600">{error}</p>
            </div>
          )}
          
          {form}
          
          {footer && (
            <div className="mt-6 text-center">
              {footer}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
```

### パターン5: Pageの作成

**要件:** ログインページのロジック（Page）を作成

**生成するファイル:**
1. `src/features/auth/pages/LoginPage.tsx`

**実装例:**
```tsx
// src/features/auth/pages/LoginPage.tsx
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useLogin } from '@/lib/api';
import { AuthFormTemplate } from '../templates/AuthFormTemplate';
import { LoginForm } from '../organisms/LoginForm';
import { Link } from '@/components/ui/link';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});
  const router = useRouter();

  const login = useLogin({
    onSuccess: () => {
      router.push('/');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // バリデーション
    const newErrors: typeof errors = {};
    if (!email) newErrors.email = 'メールアドレスを入力してください';
    if (!password) newErrors.password = 'パスワードを入力してください';
    
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    setErrors({});
    login.mutate({ email, password });
  };

  return (
    <AuthFormTemplate
      title="ログイン"
      error={login.error ? 'ログインに失敗しました' : undefined}
      form={
        <LoginForm
          email={email}
          password={password}
          onEmailChange={setEmail}
          onPasswordChange={setPassword}
          onSubmit={handleSubmit}
          isLoading={login.isPending}
          errors={errors}
        />
      }
      footer={
        <p className="text-sm text-gray-600">
          アカウントをお持ちでないですか？{' '}
          <Link href="/signup" className="text-[#88b04b] hover:underline">
            新規登録
          </Link>
        </p>
      }
    />
  );
}
```

### パターン6: 記事詳細ページの完全な実装（Page → Template → Organism）

**要件:** 記事詳細ページを5層構造で作成

**生成するファイル:**
1. `src/features/article/pages/ArticleDetailPage.tsx` (Page - ロジック)
2. `src/features/article/templates/ArticleDetailTemplate.tsx` (Template - UI構成)
3. `src/features/article/organisms/ArticleHeader.tsx` (Organism - ヘッダー部分)
4. `src/features/article/organisms/ArticleBody.tsx` (Organism - 本文部分)

**Page（ロジック）:**
```tsx
// src/features/article/pages/ArticleDetailPage.tsx
'use client';

import { useGetArticle, useToggleArticleLike } from '@/lib/api';
import { ArticleDetailTemplate } from '../templates/ArticleDetailTemplate';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleDetailPageProps {
  articleId: number;
}

export function ArticleDetailPage({ articleId }: ArticleDetailPageProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });
  const toggleLike = useToggleArticleLike();

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="記事の取得に失敗しました" onRetry={refetch} />;
  if (!data) return <ErrorMessage message="記事が見つかりません" />;

  const handleLike = () => {
    toggleLike.mutate({ articleId });
  };

  return <ArticleDetailTemplate article={data} onLike={handleLike} />;
}
```

**Template（UI構成）:**
```tsx
// src/features/article/templates/ArticleDetailTemplate.tsx
import type { GetArticleResponse } from '@/lib/api/types';
import { ArticleHeader } from '../organisms/ArticleHeader';
import { ArticleBody } from '../organisms/ArticleBody';
import { ArticleTags } from '../organisms/ArticleTags';

interface ArticleDetailTemplateProps {
  article: GetArticleResponse;
  onLike: () => void;
}

export function ArticleDetailTemplate({ article, onLike }: ArticleDetailTemplateProps) {
  return (
    <div className="min-h-screen bg-[#f5f3e8]">
      <div className="max-w-4xl mx-auto p-6">
        <article className="bg-white rounded-lg shadow-lg p-8">
          <ArticleHeader 
            title={article.title}
            authorName={article.authorName}
            likeCount={article.likeCount}
            createdAt={article.createdAt}
            onLike={onLike}
          />
          
          <ArticleBody body={article.body} />
          
          {article.tags && article.tags.length > 0 && (
            <ArticleTags tags={article.tags} />
          )}
        </article>
      </div>
    </div>
  );
}
```

**Organism（ヘッダー部分）:**
```tsx
// src/features/article/organisms/ArticleHeader.tsx
import { Button } from '@/components/ui/button';

interface ArticleHeaderProps {
  title: string;
  authorName: string;
  likeCount: number;
  createdAt: string;
  onLike: () => void;
}

export function ArticleHeader({ 
  title, 
  authorName, 
  likeCount, 
  createdAt,
  onLike 
}: ArticleHeaderProps) {
  return (
    <header className="border-b border-gray-200 pb-6 mb-6">
      <h1 className="text-3xl font-bold text-[#2d3e1f] mb-4">
        {title}
      </h1>
      
      <div className="flex items-center justify-between">
        <div className="text-sm text-gray-600">
          <span>著者: {authorName}</span>
          <span className="mx-2">•</span>
          <span>{new Date(createdAt).toLocaleDateString('ja-JP')}</span>
        </div>
        
        <Button onClick={onLike} variant="outline" size="sm">
          ❤️ {likeCount}
        </Button>
      </div>
    </header>
  );
}
```

**Organism（本文部分）:**
```tsx
// src/features/article/organisms/ArticleBody.tsx
interface ArticleBodyProps {
  body: string;
}

export function ArticleBody({ body }: ArticleBodyProps) {
  return (
    <div className="prose max-w-none mb-6">
      {body}
    </div>
  );
}
```

## よくあるパターン

### パターンA: App Routerページコンポーネントの作成

**動的ルートのページ:**
```tsx
// src/app/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailPage } from '@/features/article/pages/ArticleDetailPage';

export default function ArticleDetailRoute() {
  const params = useParams();
  const articleId = Number(params.id);

  return <ArticleDetailPage articleId={articleId} />;
}
```

### パターンB: レスポンシブデザインの実装

**Templateでレスポンシブ対応:**
```tsx
// src/features/article/templates/ArticleListTemplate.tsx
interface ArticleListTemplateProps {
  articles: Article[];
}

export function ArticleListTemplate({ articles }: ArticleListTemplateProps) {
  return (
    <div className="min-h-screen bg-[#f5f3e8]">
      {/* モバイル: 1列、タブレット: 2列、デスクトップ: 3列 */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-6">
        {articles.map((article) => (
          <ArticleCard key={article.id} article={article} />
        ))}
      </div>
    </div>
  );
}
```

### パターンC: エラーハンドリング（Page層で実装）

```tsx
// Page層でエラーハンドリング
export function ArticleDetailPage({ articleId }: { articleId: number }) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="記事の取得に失敗しました" onRetry={refetch} />;
  if (!data) return <ErrorMessage message="記事が見つかりません" />;

  return <ArticleDetailTemplate article={data} />;
}
```

## チェックリスト

新しいコンポーネントを生成する際は、以下を確認してください：

### Atomic Design関連（5層）
- [ ] 既存のAtomsで対応できないか確認したか（`components/ui/`）
- [ ] 既存のMoleculesで対応できないか確認したか（`components/molecules/`）
- [ ] 適切な階層（Atom/Molecule/Organism/Template/Page）に配置されているか
- [ ] Atomsは `components/ui/` に配置されているか
- [ ] Moleculesは `components/molecules/` に配置されているか
- [ ] Organismsは `features/{feature}/organisms/` に配置されているか
- [ ] Templatesは `features/{feature}/templates/` に配置されているか
- [ ] Pagesは `features/{feature}/pages/` に配置されているか
- [ ] レスポンシブデザインは基本的にTemplateレイヤーで実装されているか

### 責任分離
- [ ] PageはロジックのみでUIを持たないか
- [ ] TemplateはUI構成とレスポンシブ対応のみを担当しているか
- [ ] OrganismはTemplate特有のUI単位として適切に分割されているか
- [ ] MoleculeとAtomは汎用的で再利用可能か

### 基本事項
- [ ] 型定義が明確に記述されているか
- [ ] `'use client'`ディレクティブが必要な場所（Page層）に記述されているか
- [ ] 共通コンポーネント（Loading、ErrorMessage）を使用しているか
- [ ] nari-noteのブランドカラーを使用しているか
- [ ] ファイルが適切なディレクトリに配置されているか
- [ ] 命名規則に従っているか
- [ ] エラーハンドリングがPage層で実装されているか
- [ ] propsの型定義が明確か

## まとめ

### 実装の流れ

1. **Atoms確認・作成** - `components/ui/` に汎用UIコンポーネント
2. **Molecules確認・作成** - `components/molecules/` に汎用複合コンポーネント
3. **Organism作成** - `features/{feature}/organisms/` にTemplate特有のUI単位
4. **Template作成** - `features/{feature}/templates/` にUI構成とレスポンシブ対応
5. **Page作成** - `features/{feature}/pages/` にロジックとデータフェッチング
6. **App Router統合** - `app/{route}/page.tsx` からPageを呼び出し

### 各階層の役割

| 階層 | 配置 | 責務 |
|------|------|------|
| Atom | `components/ui/` | 汎用的な最小単位のUIコンポーネント |
| Molecule | `components/molecules/` | 汎用的な複合コンポーネント |
| Organism | `features/{feature}/organisms/` | Template特有のUI単位 |
| Template | `features/{feature}/templates/` | UI構成とレスポンシブ対応 |
| Page | `features/{feature}/pages/` | ロジックとデータフェッチング |

### 重要なポイント

1. **UI とロジックの分離** - PageはUIを持たず、Templateはロジックを持たない
2. **レスポンシブはTemplateで** - レスポンシブデザインは基本的にTemplateレイヤーで担当
3. **汎用性を意識** - Atom/Moleculeは複数の機能で再利用できるように設計
4. **適切な粒度** - 各コンポーネントは単一責任の原則に従う

## 関連ドキュメント

- [フロントエンドアーキテクチャガイド](./architecture.md)
- [API使用方法](./api-usage.md)
