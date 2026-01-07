# Lib

このディレクトリには、アプリケーション全体で使用される共通のロジック、ユーティリティ、プロバイダーが配置されます。

## ディレクトリ構造

```
lib/
├── api/            # API関連（既存）
├── providers/      # Reactプロバイダー（既存）
├── utils/          # ユーティリティ関数
├── hooks/          # 共通カスタムフック
└── constants/      # 定数定義
```

## API（`api/`）

APIクライアントとTanStack Query フックが配置されています。

- `client.ts` - Axiosクライアント設定
- `endpoints.ts` - APIエンドポイント定義
- `hooks.ts` - TanStack Query フック
- `types.ts` - API型定義

詳細は [API使用方法ドキュメント](/docs/frontend-api-usage.md) を参照してください。

## Providers（`providers/`）

Reactプロバイダーが配置されています。

- `QueryProvider.tsx` - TanStack Query プロバイダー
- `AuthProvider.tsx` - 認証プロバイダー（今後追加予定）

## Utils（`utils/`）

アプリケーション全体で使用されるユーティリティ関数。

**例:**
- `format.ts` - フォーマット関数（日付、数値など）
- `validation.ts` - バリデーション関数
- `date.ts` - 日付操作関数
- `string.ts` - 文字列操作関数

**使用例:**
```tsx
import { formatDate } from '@/lib/utils/format';
import { validateEmail } from '@/lib/utils/validation';

const formattedDate = formatDate(new Date());
const isValid = validateEmail('test@example.com');
```

## Hooks（`hooks/`）

アプリケーション全体で使用される共通カスタムフック。

**例:**
- `useDebounce.ts` - デバウンス処理
- `useLocalStorage.ts` - LocalStorage管理
- `useWindowSize.ts` - ウィンドウサイズ取得
- `useMediaQuery.ts` - メディアクエリ

**使用例:**
```tsx
import { useDebounce } from '@/lib/hooks/useDebounce';

const [searchTerm, setSearchTerm] = useState('');
const debouncedSearchTerm = useDebounce(searchTerm, 500);
```

## Constants（`constants/`）

アプリケーション全体で使用される定数定義。

**例:**
```tsx
// constants/index.ts
export const COLORS = {
  primary: '#88b04b',
  secondary: '#2d3e1f',
  background: '#f5f3e8',
  border: '#d4cdb3',
} as const;

export const ROUTES = {
  HOME: '/',
  ARTICLES: '/articles',
  LOGIN: '/login',
  SIGNUP: '/signup',
} as const;

export const API_ERRORS = {
  NETWORK_ERROR: 'ネットワークエラーが発生しました',
  UNAUTHORIZED: '認証が必要です',
  NOT_FOUND: '見つかりませんでした',
} as const;
```

**使用例:**
```tsx
import { COLORS, ROUTES } from '@/lib/constants';

<div style={{ backgroundColor: COLORS.primary }}>
  <Link href={ROUTES.HOME}>ホーム</Link>
</div>
```

## 機能固有のロジックとの違い

- **共通ロジック（このディレクトリ）**: 複数の機能で再利用される
- **機能固有のロジック（`features/{feature}/hooks/`）**: 特定の機能でのみ使用される

迷った場合は、まず機能固有のフックとして作成し、後で共通化を検討してください。

## 詳細

詳しくは [フロントエンドアーキテクチャガイド](/docs/frontend-architecture.md) を参照してください。
