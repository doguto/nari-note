'use client';

import { useState } from 'react';
import Link from 'next/link';

interface ApiResponse {
  token?: string;
  id?: number;
  [key: string]: unknown;
}

export default function DebugPage() {
  const [apiEndpoint, setApiEndpoint] = useState('http://localhost:5243');
  const [response, setResponse] = useState<ApiResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Auth
  const [signUpEmail, setSignUpEmail] = useState('test@example.com');
  const [signUpUsername, setSignUpUsername] = useState('testuser');
  const [signUpPassword, setSignUpPassword] = useState('Password123!');
  const [signInEmail, setSignInEmail] = useState('test@example.com');
  const [signInPassword, setSignInPassword] = useState('Password123!');
  const [authToken, setAuthToken] = useState<string>('');
  
  // Article
  const [articleTitle, setArticleTitle] = useState('テスト記事タイトル');
  const [articleContent, setArticleContent] = useState('これはテスト記事の内容です。');
  const [articleTags, setArticleTags] = useState('test,debug');
  const [articleId, setArticleId] = useState('');

  const makeRequest = async (url: string, method: string, body?: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    setResponse(null);
    
    try {
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };
      
      if (authToken) {
        headers['Authorization'] = `Bearer ${authToken}`;
      }
      
      const options: RequestInit = {
        method,
        headers,
      };
      
      if (body) {
        options.body = JSON.stringify(body);
      }
      
      const res = await fetch(url, options);
      const data = await res.json() as ApiResponse;
      
      if (!res.ok) {
        console.error(`[API Error] ${method} ${url} - HTTP ${res.status}:`, data);
        setError(`HTTP ${res.status}: ${JSON.stringify(data)}`);
      } else {
        setResponse(data);
        
        // 認証成功時にトークンを保存
        if (data.token) {
          setAuthToken(data.token);
        }
        
        // 記事作成成功時にIDを保存
        if (data.id && method === 'POST' && url.includes('/articles')) {
          setArticleId(data.id.toString());
        }
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleSignUp = () => {
    makeRequest(`${apiEndpoint}/api/auth/signup`, 'POST', {
      email: signUpEmail,
      name: signUpUsername,
      password: signUpPassword,
    });
  };

  const handleSignIn = () => {
    makeRequest(`${apiEndpoint}/api/auth/signin`, 'POST', {
      email: signInEmail,
      password: signInPassword,
    });
  };

  const handleCreateArticle = () => {
    const tags = articleTags.split(',').map(tag => tag.trim()).filter(tag => tag);
    makeRequest(`${apiEndpoint}/api/articles`, 'POST', {
      title: articleTitle,
      content: articleContent,
      tags: tags,
    });
  };

  const handleGetArticle = () => {
    if (!articleId) {
      setError('記事IDを入力してください');
      return;
    }
    makeRequest(`${apiEndpoint}/api/articles/${articleId}`, 'GET');
  };

  const handleGetArticles = () => {
    makeRequest(`${apiEndpoint}/api/articles`, 'GET');
  };

  const handleHealthCheck = () => {
    makeRequest(`${apiEndpoint}/api/health`, 'GET');
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-6xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">API Debug Console</h1>
          <Link href="/" className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
            ← ホームに戻る
          </Link>
        </div>

        {/* API Endpoint設定 */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-bold mb-4">API エンドポイント</h2>
          <div className="flex gap-4">
            <input
              type="text"
              value={apiEndpoint}
              onChange={(e) => setApiEndpoint(e.target.value)}
              className="flex-1 px-4 py-2 border rounded"
              placeholder="http://localhost:5243"
            />
            <button
              onClick={handleHealthCheck}
              className="px-6 py-2 bg-green-500 text-white rounded hover:bg-green-600"
              disabled={loading}
            >
              Health Check
            </button>
          </div>
        </div>

        {/* 認証トークン表示 */}
        {authToken && (
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-bold mb-4">認証トークン</h2>
            <div className="bg-gray-100 p-3 rounded font-mono text-sm break-all">
              {authToken}
            </div>
            <button
              onClick={() => setAuthToken('')}
              className="mt-2 px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
            >
              トークンをクリア
            </button>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* サインアップ */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">サインアップ</h2>
            <div className="space-y-3">
              <input
                type="email"
                value={signUpEmail}
                onChange={(e) => setSignUpEmail(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="Email"
              />
              <input
                type="text"
                value={signUpUsername}
                onChange={(e) => setSignUpUsername(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="Username"
              />
              <input
                type="password"
                value={signUpPassword}
                onChange={(e) => setSignUpPassword(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="Password"
              />
              <button
                onClick={handleSignUp}
                className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                disabled={loading}
              >
                サインアップ
              </button>
            </div>
          </div>

          {/* サインイン */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">サインイン</h2>
            <div className="space-y-3">
              <input
                type="email"
                value={signInEmail}
                onChange={(e) => setSignInEmail(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="Email"
              />
              <input
                type="password"
                value={signInPassword}
                onChange={(e) => setSignInPassword(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="Password"
              />
              <button
                onClick={handleSignIn}
                className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                disabled={loading}
              >
                サインイン
              </button>
            </div>
          </div>

          {/* 記事作成 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">記事作成</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={articleTitle}
                onChange={(e) => setArticleTitle(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="タイトル"
              />
              <textarea
                value={articleContent}
                onChange={(e) => setArticleContent(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="本文"
                rows={4}
              />
              <input
                type="text"
                value={articleTags}
                onChange={(e) => setArticleTags(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="タグ (カンマ区切り)"
              />
              <button
                onClick={handleCreateArticle}
                className="w-full px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
                disabled={loading || !authToken}
              >
                記事を作成
              </button>
              {!authToken && (
                <p className="text-sm text-red-500">※ 認証が必要です</p>
              )}
            </div>
          </div>

          {/* 記事取得 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">記事取得</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={articleId}
                onChange={(e) => setArticleId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="記事ID"
              />
              <button
                onClick={handleGetArticle}
                className="w-full px-4 py-2 bg-purple-500 text-white rounded hover:bg-purple-600"
                disabled={loading}
              >
                特定の記事を取得
              </button>
              <button
                onClick={handleGetArticles}
                className="w-full px-4 py-2 bg-purple-500 text-white rounded hover:bg-purple-600"
                disabled={loading}
              >
                記事一覧を取得
              </button>
            </div>
          </div>
        </div>

        {/* レスポンス表示 */}
        {(loading || response || error) && (
          <div className="bg-white rounded-lg shadow p-6 mt-6">
            <h2 className="text-xl font-bold mb-4">レスポンス</h2>
            {loading && (
              <div className="text-center py-8">
                <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
                <p className="mt-2">Loading...</p>
              </div>
            )}
            {error && (
              <div className="bg-red-50 border border-red-200 rounded p-4">
                <p className="text-red-700 font-bold">エラー:</p>
                <pre className="mt-2 text-sm text-red-600 whitespace-pre-wrap">{error}</pre>
              </div>
            )}
            {response && (
              <div className="bg-green-50 border border-green-200 rounded p-4">
                <p className="text-green-700 font-bold mb-2">成功:</p>
                <pre className="text-sm bg-white p-3 rounded overflow-auto max-h-96">
                  {JSON.stringify(response, null, 2)}
                </pre>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
