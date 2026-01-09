# Common Components

アプリケーション全体で使用される共通コンポーネントを配置するディレクトリです。

**重要**: このディレクトリはAtomic Designパターンに従って構造化されています。

## ディレクトリ構造

```
common/
├── atoms/              # 最小単位のコンポーネント
│   ├── FormField.tsx
│   ├── ErrorAlert.tsx
│   ├── FormTitle.tsx
│   ├── TagChip.tsx
│   └── index.ts
├── molecules/          # Atomsを組み合わせたコンポーネント
│   ├── EmailField.tsx
│   ├── PasswordField.tsx
│   ├── NameField.tsx
│   ├── TagInput.tsx
│   ├── CharacterCounter.tsx
│   └── index.ts
├── Loading.tsx         # ユーティリティコンポーネント
├── ErrorMessage.tsx
├── EmptyState.tsx
├── ATOMIC_DESIGN.md    # Atomic Designの詳細ドキュメント
└── README.md           # このファイル
```

詳細は `ATOMIC_DESIGN.md` を参照してください。

## Atoms（原子）

最小単位の再利用可能なUIコンポーネント。

### FormField.tsx
ラベル + 入力フィールドのセット

```tsx
import { FormField } from '@/components/common/atoms';

<FormField
  id="username"
  label="ユーザー名"
  value={username}
  onChange={setUsername}
  error={errors.username}
/>
```

### ErrorAlert.tsx
エラーメッセージ表示

```tsx
import { ErrorAlert } from '@/components/common/atoms';

<ErrorAlert message="エラーが発生しました" />
```

### FormTitle.tsx
フォームタイトル

```tsx
import { FormTitle } from '@/components/common/atoms';

<FormTitle>ログイン</FormTitle>
```

### TagChip.tsx
タグチップ（削除ボタン付き）

```tsx
import { TagChip } from '@/components/common/atoms';

<TagChip tag="TypeScript" onRemove={() => handleRemove('TypeScript')} />
```

## Molecules（分子）

複数のAtomsを組み合わせた機能コンポーネント。

### EmailField.tsx
メールアドレス入力（FormFieldを使用）

```tsx
import { EmailField } from '@/components/common/molecules';

<EmailField
  value={email}
  onChange={setEmail}
  error={errors.email}
/>
```

### PasswordField.tsx
パスワード入力（FormFieldを使用）

```tsx
import { PasswordField } from '@/components/common/molecules';

<PasswordField
  value={password}
  onChange={setPassword}
  error={errors.password}
/>
```

### NameField.tsx
ユーザー名入力（FormFieldを使用）

```tsx
import { NameField } from '@/components/common/molecules';

<NameField
  value={name}
  onChange={setName}
  error={errors.name}
/>
```

### TagInput.tsx
タグ入力（Input + Button + TagChipを使用）

```tsx
import { TagInput } from '@/components/common/molecules';

<TagInput
  tags={tags}
  onChange={setTags}
/>
```

### CharacterCounter.tsx
文字数カウンター

```tsx
import { CharacterCounter } from '@/components/common/molecules';

<CharacterCounter current={title.length} max={100} />
```

## ユーティリティコンポーネント

### Loading.tsx
ローディング表示コンポーネント

```tsx
interface LoadingProps {
  size?: 'sm' | 'md' | 'lg';
  text?: string;
}

export function Loading({ size = 'md', text = '読み込み中...' }: LoadingProps) {
  const sizeClasses = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  };

  return (
    <div className="flex flex-col items-center justify-center py-8">
      <div className={`animate-spin rounded-full border-b-2 border-[#88b04b] ${sizeClasses[size]}`} />
      {text && <p className="mt-2 text-gray-600">{text}</p>}
    </div>
  );
}
```

### ErrorMessage.tsx
エラーメッセージ表示コンポーネント

```tsx
interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  return (
    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
      <p className="text-red-700 font-medium mb-2">エラーが発生しました</p>
      <p className="text-red-600 text-sm mb-4">{message}</p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
        >
          再試行
        </button>
      )}
    </div>
  );
}
```

### EmptyState.tsx
空状態表示コンポーネント

```tsx
interface EmptyStateProps {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export function EmptyState({ title, description, icon, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12">
      {icon && <div className="text-6xl mb-4">{icon}</div>}
      <h3 className="text-xl font-bold text-gray-800 mb-2">{title}</h3>
      {description && <p className="text-gray-600 mb-4">{description}</p>}
      {action && (
        <button
          onClick={action.onClick}
          className="px-6 py-2 bg-[#88b04b] text-white rounded hover:bg-[#769939]"
        >
          {action.label}
        </button>
      )}
    </div>
  );
}
```

## 使用例

### Organism（features内）で使用

```tsx
// src/features/auth/organisms/LoginPage.tsx
import { EmailField, PasswordField } from '@/components/common/molecules';
import { ErrorAlert } from '@/components/common/atoms';
import { Loading } from '@/components/common/Loading';
import { useLogin } from '@/lib/api';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const login = useLogin();

  if (login.isLoading) {
    return <Loading />;
  }

  return (
    <form onSubmit={handleSubmit}>
      {login.error && <ErrorAlert message="ログインに失敗しました" />}
      <EmailField value={email} onChange={setEmail} />
      <PasswordField value={password} onChange={setPassword} />
      <button type="submit">ログイン</button>
    </form>
  );
}
```

### Container + Organism パターンで使用

```tsx
// Container
export function ArticleDetailContainer({ articleId }: { articleId: number }) {
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
    return (
      <EmptyState
        icon="📝"
        title="記事が見つかりません"
        description="指定された記事は存在しないか、削除された可能性があります。"
      />
    );
  }

  return <ArticleDetailPage article={data} />;
}
```

## 作成ガイドライン

### Atomsを作成する際
1. **これ以上分割できない最小単位**であることを確認
2. **shadcn UIコンポーネントまたは基本的なHTML要素**で構築
3. **他のコンポーネントに依存しない**
4. **単一責任の原則**に従う
5. **propsの型定義**を明確にする

### Moleculesを作成する際
1. **Atomsを組み合わせて**特定の機能を実装
2. **既存のAtomsで対応できないか**確認
3. **再利用可能な機能単位**として設計
4. **独自のビジネスロジックは持たない**
5. **propsで柔軟にカスタマイズ可能**にする

### 一般的なガイドライン
1. **アプリケーション全体で統一されたデザイン**を提供
2. **nari-noteのブランドカラー**（`#88b04b`, `#2d3e1f`, `#f5f3e8`）を使用
3. **適切なデフォルト値**を設定
4. **柔軟なカスタマイズ**を可能にする
5. **TypeScriptの型安全性**を活用

## その他の参考コンポーネント例

- **SuccessMessage.tsx** - 成功メッセージ
- **Pagination.tsx** - ページネーション
- **Breadcrumb.tsx** - パンくずリスト
- **Toast.tsx** - トースト通知
- **Skeleton.tsx** - スケルトンローディング
