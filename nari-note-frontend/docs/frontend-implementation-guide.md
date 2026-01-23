# フロントエンド実装ガイド（AI エージェント向け）

このドキュメントは、AIエージェントがnari-note-frontendのコードを生成する際の具体的なガイドラインです。

**重要**: nari-noteではAtomic Designパターンを採用しています。コンポーネントを小さな単位（Atoms → Molecules → Organisms）で実装してください。

## 目次

1. [コード生成の基本ルール](#コード生成の基本ルール)
2. [ディレクトリ配置ルール](#ディレクトリ配置ルール)
3. [命名規則](#命名規則)
4. [コンポーネント生成パターン](#コンポーネント生成パターン)
5. [よくあるパターン](#よくあるパターン)

## コード生成の基本ルール

### 1. Atomic Designパターンを必ず使用

コンポーネントを小さな単位で階層的に実装してください。

#### 実装の順序

1. **Atomsの確認・作成** (`components/common/atoms/`)
   - 必要な基本要素が既に存在するか確認
   - 存在しない場合は新規作成
   - 例: FormField, ErrorAlert, TagChip

2. **Moleculesの確認・作成** (`components/common/molecules/`)
   - Atomsを組み合わせて機能コンポーネントを作成
   - 既存のMoleculesで対応できないか確認
   - 例: EmailField, PasswordField, TagInput

3. **Organismsの実装** (`features/{feature}/organisms/`)
   - Atoms/Moleculesを組み合わせて完全な機能を実装
   - データフェッチングやビジネスロジックを含む
   - 例: LoginPage, ArticleFormPage

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

// ✅ 良い例: Atomic Designで小さく分割
// Atom: FormField (components/common/atoms/FormField.tsx)
export function FormField({ id, label, type, value, onChange, error }: FormFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>{label}</Label>
      <Input id={id} type={type} value={value} onChange={(e) => onChange(e.target.value)} />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}

// Molecule: EmailField (components/common/molecules/EmailField.tsx)
export function EmailField({ value, onChange, error }: EmailFieldProps) {
  return (
    <FormField
      id="email"
      label="メールアドレス"
      type="email"
      value={value}
      onChange={onChange}
      error={error}
    />
  );
}

// Molecule: PasswordField (components/common/molecules/PasswordField.tsx)
export function PasswordField({ value, onChange, error }: PasswordFieldProps) {
  return (
    <FormField
      id="password"
      label="パスワード"
      type="password"
      value={value}
      onChange={onChange}
      error={error}
    />
  );
}

// Organism: LoginPage (features/auth/organisms/LoginPage.tsx)
export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const login = useLogin();
  
  return (
    <form onSubmit={handleSubmit}>
      <EmailField value={email} onChange={setEmail} />
      <PasswordField value={password} onChange={setPassword} />
      <Button type="submit">ログイン</Button>
    </form>
  );
}
```

### 2. Container/Presentationalパターン（必要に応じて）

Organismsが複雑になる場合や、データフェッチングロジックが大きい場合はContainer/Presentationalに分離してください。

```tsx
// Container
export function ArticleDetailContainer({ articleId }: { articleId: number }) {
  const { data, isLoading, error } = useGetArticle({ id: articleId });
  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="エラー" />;
  if (!data) return null;
  return <ArticleDetailPage article={data} />;
}

// Presentational (Organism)
export function ArticleDetailPage({ article }: { article: GetArticleResponse }) {
  return <article>{/* ... */}</article>;
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

## ディレクトリ配置ルール

### Atomic Design構造

```
src/
├── components/
│   ├── common/
│   │   ├── atoms/              # 最小単位のコンポーネント
│   │   │   ├── FormField.tsx
│   │   │   ├── ErrorAlert.tsx
│   │   │   ├── FormTitle.tsx
│   │   │   ├── TagChip.tsx
│   │   │   └── index.ts
│   │   ├── molecules/          # Atomsを組み合わせたコンポーネント
│   │   │   ├── EmailField.tsx
│   │   │   ├── PasswordField.tsx
│   │   │   ├── NameField.tsx
│   │   │   ├── TagInput.tsx
│   │   │   ├── CharacterCounter.tsx
│   │   │   └── index.ts
│   │   ├── Loading.tsx
│   │   ├── ErrorMessage.tsx
│   │   └── EmptyState.tsx
│   ├── ui/                     # 基本UIコンポーネント（shadcn/ui等）
│   └── layout/                 # レイアウトコンポーネント
├── features/
│   ├── article/
│   │   ├── organisms/          # 完全な機能ブロック
│   │   │   ├── ArticleFormPage.tsx
│   │   │   ├── ArticleDetailPage.tsx
│   │   │   └── HomeArticleList.tsx
│   │   └── types.ts
│   ├── auth/
│   │   ├── organisms/
│   │   │   ├── LoginPage.tsx
│   │   │   └── SignUpPage.tsx
│   │   └── types.ts
│   ├── user/
│   │   ├── organisms/
│   │   │   └── UserProfilePage.tsx
│   │   └── types.ts
│   └── tag/
│       └── organisms/
│           └── TagArticleListPage.tsx
└── app/                        # Next.js App Router
    └── ...
```

### 配置ルール

| 何を作る？ | どこに配置？ | 例 |
|-----------|------------|-----|
| 最小単位のコンポーネント | `src/components/common/atoms/` | `FormField.tsx`, `ErrorAlert.tsx` |
| 機能コンポーネント | `src/components/common/molecules/` | `EmailField.tsx`, `TagInput.tsx` |
| 完全な機能ブロック | `src/features/{feature}/organisms/` | `LoginPage.tsx`, `ArticleFormPage.tsx` |
| 基本UIコンポーネント | `src/components/ui/` | `Button.tsx`, `Input.tsx` |
| レイアウトコンポーネント | `src/components/layout/` | `Header.tsx`, `Footer.tsx` |
| ユーティリティコンポーネント | `src/components/common/` | `Loading.tsx`, `ErrorMessage.tsx` |
| ページコンポーネント | `src/app/{route}/` | `page.tsx` |
| ユーティリティ関数 | `src/lib/utils/` | `format.ts`, `validation.ts` |
| カスタムフック（共通） | `src/lib/hooks/` | `useDebounce.ts` |

## 命名規則

### コンポーネントファイル

| 種類 | 命名規則 | 例 |
|------|----------|-----|
| Presentational Component | `{ComponentName}.tsx` | `ArticleCard.tsx` |
| Container Component | `{ComponentName}Container.tsx` | `ArticleCardContainer.tsx` |
| Page Component | `page.tsx` | `page.tsx` |
| Layout Component | `layout.tsx` | `layout.tsx` |

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

**要件:** フォームフィールドの基本コンポーネントを作成

**生成するファイル:**
1. `src/components/common/atoms/FormField.tsx`

**実装例:**
```tsx
// src/components/common/atoms/FormField.tsx
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface FormFieldProps {
  id: string;
  label: string;
  type?: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  error?: string;
  required?: boolean;
}

export function FormField({
  id,
  label,
  type = 'text',
  value,
  onChange,
  placeholder,
  error,
  required = false,
}: FormFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </Label>
      <Input
        id={id}
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className={error ? 'border-red-500' : ''}
      />
      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}
```

### パターン2: Moleculeの作成

**要件:** メールアドレス入力フィールドを作成

**生成するファイル:**
1. `src/components/common/molecules/EmailField.tsx`

**実装例:**
```tsx
// src/components/common/molecules/EmailField.tsx
import { FormField } from '@/components/common/atoms/FormField';

interface EmailFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  required?: boolean;
}

export function EmailField({ 
  value, 
  onChange, 
  error,
  required = true,
}: EmailFieldProps) {
  return (
    <FormField
      id="email"
      label="メールアドレス"
      type="email"
      value={value}
      onChange={onChange}
      placeholder="example@example.com"
      error={error}
      required={required}
    />
  );
}
```

### パターン3: Organism（シンプルなフォーム）の作成

**要件:** ログインフォームを作成

**生成するファイル:**
1. `src/features/auth/organisms/LoginPage.tsx`

**実装例:**
```tsx
// src/features/auth/organisms/LoginPage.tsx
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { EmailField } from '@/components/common/molecules/EmailField';
import { PasswordField } from '@/components/common/molecules/PasswordField';
import { ErrorAlert } from '@/components/common/atoms/ErrorAlert';
import { Button } from '@/components/ui/button';
import { useLogin } from '@/lib/api';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});
  const router = useRouter();

  const login = useLogin({
    onSuccess: () => {
      router.push('/');
    },
    onError: (error) => {
      // エラーハンドリング
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

    login.mutate({ email, password });
  };

  return (
    <div className="max-w-md mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">ログイン</h1>
      
      {login.error && (
        <ErrorAlert message="ログインに失敗しました" />
      )}
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <EmailField 
          value={email} 
          onChange={setEmail} 
          error={errors.email}
        />
        <PasswordField 
          value={password} 
          onChange={setPassword}
          error={errors.password}
        />
        <Button 
          type="submit" 
          disabled={login.isPending}
          className="w-full"
        >
          {login.isPending ? 'ログイン中...' : 'ログイン'}
        </Button>
      </form>
    </div>
  );
}
```

### パターン4: Organism（データ取得を伴う）の作成

**要件:** 記事詳細ページを作成

**生成するファイル:**
1. `src/features/article/organisms/ArticleDetailPage.tsx` (Presentational Organism)
2. `src/features/article/organisms/ArticleDetailContainer.tsx` (Container) - 必要に応じて

**Presentational Organism:**
```tsx
// src/features/article/organisms/ArticleDetailPage.tsx
import type { GetArticleResponse } from '@/lib/api/types';
import { TagChip } from '@/components/common/atoms/TagChip';

interface ArticleDetailPageProps {
  article: GetArticleResponse;
  onLike?: () => void;
}

export function ArticleDetailPage({ article, onLike }: ArticleDetailPageProps) {
  return (
    <article className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold text-[#2d3e1f] mb-4">
        {article.title}
      </h1>
      
      <div className="flex items-center gap-4 mb-6 text-gray-600">
        <span>著者: {article.authorName}</span>
        <span>いいね: {article.likeCount}</span>
        {onLike && (
          <button 
            onClick={onLike}
            className="px-4 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939]"
          >
            いいね
          </button>
        )}
      </div>
      
      <div className="prose max-w-none mb-6">
        {article.body}
      </div>
      
      {article.tags && article.tags.length > 0 && (
        <div className="flex gap-2">
          {article.tags.map((tag, index) => (
            <TagChip key={index} tag={tag} />
          ))}
        </div>
      )}
    </article>
  );
}
```

**Container（必要に応じて）:**
```tsx
// src/features/article/organisms/ArticleDetailContainer.tsx
'use client';

import { useGetArticle, useToggleArticleLike } from '@/lib/api';
import { ArticleDetailPage } from './ArticleDetailPage';
import { Loading } from '@/components/common/Loading';
import { ErrorMessage } from '@/components/common/ErrorMessage';

interface ArticleDetailContainerProps {
  articleId: number;
}

export function ArticleDetailContainer({ articleId }: ArticleDetailContainerProps) {
  const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });
  const toggleLike = useToggleArticleLike();

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <ErrorMessage 
        message="記事の取得に失敗しました" 
        onRetry={refetch}
      />
    );
  }

  if (!data) {
    return <ErrorMessage message="記事が見つかりません" />;
  }

  const handleLike = () => {
    toggleLike.mutate({ articleId });
  };

  return <ArticleDetailPage article={data} onLike={handleLike} />;
}
```

### パターン5: 複雑なOrganism（フォーム + カスタムフック）

**要件:** 記事作成フォームを作成

**生成するファイル:**
1. `src/components/common/molecules/TitleField.tsx` (Molecule)
2. `src/components/common/molecules/BodyField.tsx` (Molecule)
3. `src/features/article/organisms/ArticleFormPage.tsx` (Organism)

**Molecule: TitleField:**
```tsx
// src/components/common/molecules/TitleField.tsx
import { FormField } from '@/components/common/atoms/FormField';
import { CharacterCounter } from '@/components/common/molecules/CharacterCounter';

interface TitleFieldProps {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  maxLength?: number;
}

export function TitleField({ 
  value, 
  onChange, 
  error,
  maxLength = 100,
}: TitleFieldProps) {
  return (
    <div className="space-y-2">
      <FormField
        id="title"
        label="タイトル"
        value={value}
        onChange={onChange}
        error={error}
        required
        placeholder="記事のタイトルを入力"
      />
      <CharacterCounter current={value.length} max={maxLength} />
    </div>
  );
}
```

**Organism: ArticleFormPage:**
```tsx
// src/features/article/organisms/ArticleFormPage.tsx
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { TitleField } from '@/components/common/molecules/TitleField';
import { BodyField } from '@/components/common/molecules/BodyField';
import { TagInput } from '@/components/common/molecules/TagInput';
// または既存のFormFieldを直接使用する場合：
// import { FormField } from '@/components/common/atoms/FormField';
import { ErrorAlert } from '@/components/common/atoms/ErrorAlert';
import { Button } from '@/components/ui/button';
import { useCreateArticle } from '@/lib/api';

export function ArticleFormPage() {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [errors, setErrors] = useState<{ title?: string; body?: string }>({});
  
  const router = useRouter();
  
  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      router.push(`/articles/${data.id}`);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // バリデーション
    const newErrors: typeof errors = {};
    if (!title) newErrors.title = 'タイトルを入力してください';
    if (!body) newErrors.body = '本文を入力してください';
    
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    createArticle.mutate({ title, body, tags });
  };

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">新規記事作成</h1>
      
      {createArticle.error && (
        <ErrorAlert message="記事の作成に失敗しました" />
      )}
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <TitleField 
          value={title} 
          onChange={setTitle} 
          error={errors.title}
        />
        
        <BodyField 
          value={body} 
          onChange={setBody}
          error={errors.body}
        />
        
        <TagInput 
          tags={tags} 
          onChange={setTags}
        />
        
        <Button 
          type="submit" 
          disabled={createArticle.isPending}
          className="w-full"
        >
          {createArticle.isPending ? '作成中...' : '記事を作成'}
        </Button>
      </form>
    </div>
  );
}
```

## よくあるパターン

### パターンA: ページコンポーネントの作成

**動的ルートのページ:**
```tsx
// src/app/articles/[id]/page.tsx
'use client';

import { useParams } from 'next/navigation';
import { ArticleDetailContainer } from '@/features/article/organisms/ArticleDetailContainer';

export default function ArticleDetailPage() {
  const params = useParams();
  const articleId = Number(params.id);

  return (
    <div className="container mx-auto">
      <ArticleDetailContainer articleId={articleId} />
    </div>
  );
}
```

### パターンB: 認証が必要なページ

```tsx
// src/app/articles/new/page.tsx
import { ArticleFormPage } from '@/features/article/organisms/ArticleFormPage';

export default function NewArticlePage() {
  return (
    <div className="container mx-auto max-w-4xl">
      <ArticleFormPage />
    </div>
  );
}
```

### パターンC: エラーハンドリング

```tsx
const { data, isLoading, error, refetch } = useGetArticle({ id: articleId });

if (isLoading) {
  return <Loading />;
}

if (error) {
  return (
    <ErrorMessage 
      message="記事の取得に失敗しました" 
      onRetry={refetch}
    />
  );
}

if (!data) {
  return <ErrorMessage message="記事が見つかりません" />;
}

return <ArticleDetailPage article={data} />;
```

## チェックリスト

新しいコンポーネントを生成する際は、以下を確認してください：

### Atomic Design関連
- [ ] 既存のAtomsで対応できないか確認したか
- [ ] 既存のMoleculesで対応できないか確認したか
- [ ] コンポーネントを小さな単位（Atoms → Molecules → Organisms）で分割しているか
- [ ] Atomsは `components/common/atoms/` に配置されているか
- [ ] Moleculesは `components/common/molecules/` に配置されているか
- [ ] Organismsは `features/{feature}/organisms/` に配置されているか

### 基本事項
- [ ] 型定義が明確に記述されているか
- [ ] `'use client'`ディレクティブが必要な場所（データフェッチングを含むOrganism）に記述されているか
- [ ] 共通コンポーネント（Loading、ErrorMessage）を使用しているか
- [ ] nari-noteのブランドカラーを使用しているか
- [ ] ファイルが適切なディレクトリに配置されているか
- [ ] 命名規則に従っているか
- [ ] エラーハンドリングが実装されているか
- [ ] propsの型定義が明確か

## 関連ドキュメント

- [フロントエンドアーキテクチャガイド](./frontend-architecture.md)
- [API使用方法](./frontend-api-usage.md)
