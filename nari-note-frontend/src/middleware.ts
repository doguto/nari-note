import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// 認証が必要なパスのパターン
const protectedPaths = [
  '/articles/new',
  '/articles/drafts',
  '/articles/my-articles',
  '/articles/:id/edit',
  '/settings/profile',
];

// パスが保護されているかチェック
function isProtectedPath(pathname: string): boolean {
  return protectedPaths.some(pattern => {
    // :id などの動的パラメータを正規表現に変換
    const regexPattern = pattern.replace(/:\w+/g, '[^/]+');
    const regex = new RegExp(`^${regexPattern}$`);
    return regex.test(pathname);
  });
}

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // 保護されたパスでない場合はそのまま通す
  if (!isProtectedPath(pathname)) {
    return NextResponse.next();
  }

  // Cookieから認証トークンを確認
  const authToken = request.cookies.get('authToken');

  // 認証トークンがない場合、ログインページにリダイレクト
  if (!authToken) {
    const loginUrl = new URL('/login', request.url);
    // 元のパスをリダイレクトパラメータとして保存
    loginUrl.searchParams.set('redirect', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // 認証済みの場合はそのまま通す
  return NextResponse.next();
}

// middlewareを適用するパスの設定
export const config = {
  matcher: [
    '/articles/new',
    '/articles/drafts',
    '/articles/my-articles',
    '/articles/:id*/edit',
    '/settings/profile',
  ],
};
