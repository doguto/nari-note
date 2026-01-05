'use client'

import { useState } from 'react'
import {
  Button,
  Input,
  Textarea,
  Label,
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/components/ui'

export default function UiDemoPage() {
  const [inputValue, setInputValue] = useState('')
  const [textareaValue, setTextareaValue] = useState('')

  return (
    <div className="container mx-auto p-8 space-y-8">
      <div className="space-y-4">
        <h1 className="text-4xl font-bold">shadcn/ui コンポーネントデモ</h1>
        <p className="text-[hsl(var(--muted-foreground))]">
          Nari-noteに導入されたshadcn/uiコンポーネントのデモページです。
        </p>
      </div>

      {/* Button variants */}
      <Card>
        <CardHeader>
          <CardTitle>Button</CardTitle>
          <CardDescription>様々なバリエーションのボタンコンポーネント</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <div className="font-medium text-sm">Variants</div>
            <div className="flex flex-wrap gap-2">
              <Button variant="default">Default</Button>
              <Button variant="secondary">Secondary</Button>
              <Button variant="destructive">Destructive</Button>
              <Button variant="outline">Outline</Button>
              <Button variant="ghost">Ghost</Button>
              <Button variant="link">Link</Button>
            </div>
          </div>
          
          <div className="space-y-2">
            <div className="font-medium text-sm">Sizes</div>
            <div className="flex flex-wrap items-center gap-2">
              <Button size="sm">Small</Button>
              <Button size="default">Default</Button>
              <Button size="lg">Large</Button>
            </div>
          </div>
          
          <div className="space-y-2">
            <div className="font-medium text-sm">States</div>
            <div className="flex flex-wrap gap-2">
              <Button disabled>Disabled</Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Input */}
      <Card>
        <CardHeader>
          <CardTitle>Input</CardTitle>
          <CardDescription>テキスト入力フィールド</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">メールアドレス</Label>
            <Input
              id="email"
              type="email"
              placeholder="example@example.com"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
            />
          </div>
          
          <div className="space-y-2">
            <Label htmlFor="password">パスワード</Label>
            <Input
              id="password"
              type="password"
              placeholder="パスワードを入力"
            />
          </div>
          
          <div className="space-y-2">
            <Label htmlFor="disabled">無効化された入力</Label>
            <Input
              id="disabled"
              type="text"
              placeholder="入力できません"
              disabled
            />
          </div>
        </CardContent>
      </Card>

      {/* Textarea */}
      <Card>
        <CardHeader>
          <CardTitle>Textarea</CardTitle>
          <CardDescription>複数行テキスト入力エリア</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="message">メッセージ</Label>
            <Textarea
              id="message"
              placeholder="メッセージを入力してください"
              value={textareaValue}
              onChange={(e) => setTextareaValue(e.target.value)}
              rows={5}
            />
          </div>
          
          <div className="space-y-2">
            <Label htmlFor="disabled-textarea">無効化されたテキストエリア</Label>
            <Textarea
              id="disabled-textarea"
              placeholder="入力できません"
              disabled
              rows={3}
            />
          </div>
        </CardContent>
      </Card>

      {/* Card */}
      <Card>
        <CardHeader>
          <CardTitle>Card</CardTitle>
          <CardDescription>カードコンポーネントの構造例</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>カードタイトル</CardTitle>
                <CardDescription>カードの説明文がここに入ります</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm">カードのコンテンツエリアです。</p>
              </CardContent>
              <CardFooter>
                <Button>アクション</Button>
              </CardFooter>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>もう一つのカード</CardTitle>
                <CardDescription>別のカード例</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm">ネストされたカードも表示できます。</p>
              </CardContent>
              <CardFooter className="gap-2">
                <Button variant="outline">キャンセル</Button>
                <Button>確定</Button>
              </CardFooter>
            </Card>
          </div>
        </CardContent>
      </Card>

      {/* Form example */}
      <Card>
        <CardHeader>
          <CardTitle>フォーム例</CardTitle>
          <CardDescription>全てのコンポーネントを組み合わせた例</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="form-name">名前</Label>
              <Input id="form-name" placeholder="山田太郎" />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="form-email">メールアドレス</Label>
              <Input id="form-email" type="email" placeholder="taro@example.com" />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="form-message">お問い合わせ内容</Label>
              <Textarea
                id="form-message"
                placeholder="お問い合わせ内容を入力してください"
                rows={4}
              />
            </div>
          </form>
        </CardContent>
        <CardFooter className="gap-2">
          <Button variant="outline">クリア</Button>
          <Button>送信</Button>
        </CardFooter>
      </Card>
    </div>
  )
}
