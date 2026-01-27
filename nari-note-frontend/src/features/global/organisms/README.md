# Layout Components

ページのレイアウト構造を提供するコンポーネントを配置するディレクトリです。

## 概要

このディレクトリには、アプリケーションのレイアウトを構成するコンポーネントを配置します。
ヘッダー、フッター、サイドバーなど、ページの基本構造を提供するコンポーネントです。

## コンポーネント例

### Header.tsx
アプリケーションのヘッダー

```tsx
interface HeaderProps {
  isLoggedIn?: boolean;
  userName?: string;
}

export function Header({ isLoggedIn, userName }: HeaderProps) {
  return (
    <header className="bg-[#f5f3e8] border-b border-[#d4cdb3] shadow-sm">
      <div className="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-[#88b04b] rounded-lg flex items-center justify-center text-white font-bold">
            な
          </div>
          <span className="text-xl font-bold text-[#2d3e1f]">なりノート</span>
        </div>
        
        <nav className="flex items-center gap-8">
          <Link href="/">ホーム</Link>
          <Link href="/articles">記事</Link>
          {isLoggedIn ? (
            <span>ようこそ、{userName}さん</span>
          ) : (
            <Link href="/login">ログイン</Link>
          )}
        </nav>
      </div>
    </header>
  );
}
```

### Footer.tsx
アプリケーションのフッター

```tsx
export function Footer() {
  return (
    <footer className="bg-[#2d3e1f] text-white py-8 mt-16">
      <div className="max-w-7xl mx-auto px-4 text-center">
        <p>© 2024 なりノート</p>
      </div>
    </footer>
  );
}
```

### Sidebar.tsx
サイドバー

```tsx
interface SidebarProps {
  children: React.ReactNode;
}

export function Sidebar({ children }: SidebarProps) {
  return (
    <aside className="w-80 hidden lg:block">
      {children}
    </aside>
  );
}
```

## 使用例

### app/layout.tsx で使用

```tsx
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';

export default function RootLayout({ children }) {
  return (
    <html lang="ja">
      <body>
        <Header />
        <main className="min-h-screen">
          {children}
        </main>
        <Footer />
      </body>
    </html>
  );
}
```

### ページで使用

```tsx
import { Sidebar } from '@/components/layout/Sidebar';

export default function HomePage() {
  return (
    <div className="max-w-7xl mx-auto px-4">
      <div className="flex gap-8">
        <main className="flex-1">
          {/* メインコンテンツ */}
        </main>
        <Sidebar>
          {/* サイドバーコンテンツ */}
        </Sidebar>
      </div>
    </div>
  );
}
```

## 作成ガイドライン

1. **レスポンシブデザイン**を考慮
2. **nari-noteのブランドカラー**を使用
3. **一貫性のあるデザイン**を維持
4. **アクセシビリティ**を考慮

## nari-noteのデザインカラー

- プライマリ: `#88b04b` (緑)
- セカンダリ: `#2d3e1f` (ダークグリーン)
- 背景: `#f5f3e8` (ベージュ)
- ボーダー: `#d4cdb3` (ライトブラウン)
