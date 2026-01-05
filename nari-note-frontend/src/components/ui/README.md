# UI Components

基本的なUIコンポーネントを配置するディレクトリです。

## 概要

このディレクトリには、アプリケーション全体で再利用される基本的なUIコンポーネントを配置します。
ボタン、入力フィールド、カードなど、機能に依存しない汎用的なコンポーネントです。

## コンポーネント例

### Button.tsx
ボタンコンポーネント

```tsx
interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  type?: 'button' | 'submit' | 'reset';
}
```

### Input.tsx
入力フィールドコンポーネント

```tsx
interface InputProps {
  type?: 'text' | 'email' | 'password' | 'number';
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  label?: string;
  error?: string;
  disabled?: boolean;
}
```

### Card.tsx
カードコンポーネント

```tsx
interface CardProps {
  children: React.ReactNode;
  className?: string;
  padding?: 'sm' | 'md' | 'lg';
}
```

### Modal.tsx
モーダルコンポーネント

```tsx
interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
}
```

## shadcn/ui について

将来的に shadcn/ui を導入する場合、このディレクトリに配置します。

```bash
npx shadcn-ui@latest add button
```

生成されたコンポーネントは `src/components/ui/` に配置されます。

## 作成ガイドライン

1. **Presentationalコンポーネント**として作成
2. **propsで柔軟にカスタマイズ**可能にする
3. **Tailwind CSS**でスタイリング
4. **型定義**を明確にする
5. **デフォルト値**を適切に設定

## 使用例

```tsx
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';

export function MyForm() {
  return (
    <Card>
      <Input
        label="メールアドレス"
        type="email"
        value={email}
        onChange={setEmail}
      />
      <Button variant="primary" type="submit">
        送信
      </Button>
    </Card>
  );
}
```
