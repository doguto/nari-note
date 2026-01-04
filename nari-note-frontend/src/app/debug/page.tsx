'use client';

import { useState } from 'react';
import Link from 'next/link';
import { 
  authApi, 
  articlesApi, 
  usersApi, 
  healthApi,
  type AuthResponse,
  type CreateArticleResponse,
  type GetArticleResponse,
  type GetArticlesByAuthorResponse,
  type GetArticlesByTagResponse,
  type UpdateArticleResponse,
  type ToggleLikeResponse,
  type GetUserProfileResponse,
} from '@/lib/api';
import { AxiosError } from 'axios';

export default function DebugPage() {
  const [response, setResponse] = useState<Record<string, unknown> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Auth
  const [signUpEmail, setSignUpEmail] = useState('test@example.com');
  const [signUpUsername, setSignUpUsername] = useState('testuser');
  const [signUpPassword, setSignUpPassword] = useState('Password123!');
  const [signInEmail, setSignInEmail] = useState('test@example.com');
  const [signInPassword, setSignInPassword] = useState('Password123!');
  
  // Article
  const [articleTitle, setArticleTitle] = useState('テスト記事タイトル');
  const [articleContent, setArticleContent] = useState('これはテスト記事の内容です。');
  const [articleTags, setArticleTags] = useState('test,debug');
  const [articleId, setArticleId] = useState('');
  
  // User Profile
  const [userId, setUserId] = useState('');
  const [userName, setUserName] = useState('');
  const [userBio, setUserBio] = useState('');
  const [userProfileImage, setUserProfileImage] = useState('');
  
  // Search
  const [authorId, setAuthorId] = useState('');
  const [tagName, setTagName] = useState('');

  const handleApiCall = async <T,>(
    apiCall: () => Promise<T>,
    successCallback?: (data: T) => void
  ) => {
    setLoading(true);
    setError(null);
    setResponse(null);
    
    try {
      const data = await apiCall();
      setResponse(data as Record<string, unknown>);
      
      if (successCallback) {
        successCallback(data);
      }
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      const errorMessage = axiosError.response?.data?.message || axiosError.message || 'Unknown error occurred';
      const statusCode = axiosError.response?.status;
      const fullError = `HTTP ${statusCode}: ${errorMessage}`;
      
      console.error('[API Error]:', fullError, axiosError.response?.data);
      setError(fullError);
      alert(`❌ エラーが発生しました\n\n${fullError}`);
    } finally {
      setLoading(false);
    }
  };

  const handleSignUp = () => {
    handleApiCall(
      () => authApi.signUp({
        email: signUpEmail,
        name: signUpUsername,
        password: signUpPassword,
      }),
      (data: AuthResponse) => {
        if (data.userId) {
          setUserId(data.userId.toString());
        }
      }
    );
  };

  const handleSignIn = () => {
    handleApiCall(
      () => authApi.signIn({
        usernameOrEmail: signInEmail,
        password: signInPassword,
      }),
      (data: AuthResponse) => {
        if (data.userId) {
          setUserId(data.userId.toString());
        }
      }
    );
  };

  const handleCreateArticle = () => {
    const tags = articleTags.split(',').map(tag => tag.trim()).filter(tag => tag);
    handleApiCall(
      () => articlesApi.createArticle({
        title: articleTitle,
        body: articleContent,
        tags: tags,
      }),
      (data: CreateArticleResponse) => {
        if (data.id) {
          setArticleId(data.id.toString());
        }
      }
    );
  };

  const handleGetArticle = () => {
    if (!articleId) {
      const errorMsg = '記事IDを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    handleApiCall(
      () => articlesApi.getArticle({ id: parseInt(articleId) }),
      (data: GetArticleResponse) => {
        if (data.id) {
          setArticleId(data.id.toString());
        }
        if (data.authorId) {
          setUserId(data.authorId.toString());
        }
      }
    );
  };

  const handleGetArticles = () => {
    alert('❌ GET /api/articles エンドポイントはバックエンドに実装されていません。\n代わりに「著者別記事取得」または「タグ別記事取得」を使用してください。');
  };

  const handleHealthCheck = () => {
    handleApiCall(() => healthApi.getHealth());
  };

  const handleUpdateArticle = () => {
    if (!articleId) {
      const errorMsg = '記事IDを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    const tags = articleTags.split(',').map(tag => tag.trim()).filter(tag => tag);
    handleApiCall(
      () => articlesApi.updateArticle({
        id: parseInt(articleId),
        title: articleTitle,
        body: articleContent,
        tags: tags,
      }),
      (data: UpdateArticleResponse) => {
        if (data.id) {
          setArticleId(data.id.toString());
        }
      }
    );
  };

  const handleDeleteArticle = () => {
    if (!articleId) {
      const errorMsg = '記事IDを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    handleApiCall(() => articlesApi.deleteArticle({ id: parseInt(articleId) }));
  };

  const handleToggleLike = () => {
    if (!articleId) {
      const errorMsg = '記事IDを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    handleApiCall(
      () => articlesApi.toggleLike({ articleId: parseInt(articleId) }),
      (data: ToggleLikeResponse) => {
        console.log('Like toggled:', data);
      }
    );
  };

  const handleGetUserProfile = () => {
    if (!userId) {
      const errorMsg = 'ユーザーIDを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    handleApiCall(
      () => usersApi.getUserProfile({ id: parseInt(userId) }),
      (data: GetUserProfileResponse) => {
        if (data.id) {
          setUserId(data.id.toString());
        }
      }
    );
  };

  const handleUpdateUserProfile = () => {
    const body: { name?: string; bio?: string; profileImage?: string } = {};
    if (userName) body.name = userName;
    if (userBio) body.bio = userBio;
    if (userProfileImage) body.profileImage = userProfileImage;
    
    if (Object.keys(body).length === 0) {
      const errorMsg = '少なくとも1つのフィールドを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    
    handleApiCall(() => usersApi.updateUserProfile(body));
  };

  const handleGetArticlesByAuthor = () => {
    if (!authorId) {
      const errorMsg = '著者IDを入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    handleApiCall(
      () => articlesApi.getArticlesByAuthor({ authorId: parseInt(authorId) }),
      (data: GetArticlesByAuthorResponse) => {
        console.log('Articles by author:', data);
      }
    );
  };

  const handleGetArticlesByTag = () => {
    if (!tagName) {
      const errorMsg = 'タグ名を入力してください';
      setError(errorMsg);
      alert(`❌ ${errorMsg}`);
      return;
    }
    handleApiCall(
      () => articlesApi.getArticlesByTag({ tagName }),
      (data: GetArticlesByTagResponse) => {
        console.log('Articles by tag:', data);
      }
    );
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

        {/* Health Check */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-bold mb-4">Health Check</h2>
          <button
            onClick={handleHealthCheck}
            className="px-6 py-2 bg-green-500 text-white rounded hover:bg-green-600"
            disabled={loading}
          >
            Health Check
          </button>
        </div>

        {/* 認証情報 */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-bold mb-4">認証について</h2>
          <p className="text-sm text-gray-600">
            認証はCookieベースで自動管理されます。サインアップ/サインイン後は自動的に認証されます。
          </p>
        </div>

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
                disabled={loading}
              >
                記事を作成
              </button>
              <p className="text-sm text-gray-500">※ 認証が必要です（自動でトークンが使用されます）</p>
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

          {/* 記事更新 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">記事更新</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={articleId}
                onChange={(e) => setArticleId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="記事ID"
              />
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
                onClick={handleUpdateArticle}
                className="w-full px-4 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600"
                disabled={loading}
              >
                記事を更新
              </button>
              <p className="text-sm text-gray-500">※ 認証が必要です（自動でトークンが使用されます）</p>
            </div>
          </div>

          {/* 記事削除 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">記事削除</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={articleId}
                onChange={(e) => setArticleId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="記事ID"
              />
              <button
                onClick={handleDeleteArticle}
                className="w-full px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
                disabled={loading}
              >
                記事を削除
              </button>
              <p className="text-sm text-gray-500">※ 認証が必要です（自動でトークンが使用されます）</p>
            </div>
          </div>

          {/* いいね切り替え */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">いいね切り替え</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={articleId}
                onChange={(e) => setArticleId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="記事ID"
              />
              <button
                onClick={handleToggleLike}
                className="w-full px-4 py-2 bg-pink-500 text-white rounded hover:bg-pink-600"
                disabled={loading}
              >
                いいねを切り替え
              </button>
              <p className="text-sm text-gray-500">※ 認証が必要です（自動でトークンが使用されます）</p>
            </div>
          </div>

          {/* ユーザープロフィール取得 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">ユーザープロフィール取得</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="ユーザーID"
              />
              <button
                onClick={handleGetUserProfile}
                className="w-full px-4 py-2 bg-indigo-500 text-white rounded hover:bg-indigo-600"
                disabled={loading}
              >
                プロフィールを取得
              </button>
            </div>
          </div>

          {/* ユーザープロフィール更新 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">ユーザープロフィール更新</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="ユーザー名"
              />
              <textarea
                value={userBio}
                onChange={(e) => setUserBio(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="自己紹介"
                rows={3}
              />
              <input
                type="text"
                value={userProfileImage}
                onChange={(e) => setUserProfileImage(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="プロフィール画像URL"
              />
              <button
                onClick={handleUpdateUserProfile}
                className="w-full px-4 py-2 bg-indigo-500 text-white rounded hover:bg-indigo-600"
                disabled={loading}
              >
                プロフィールを更新
              </button>
              <p className="text-sm text-gray-500">※ 認証が必要です（自動でトークンが使用されます）</p>
            </div>
          </div>

          {/* 著者別記事取得 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">著者別記事取得</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={authorId}
                onChange={(e) => setAuthorId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="著者ID"
              />
              <button
                onClick={handleGetArticlesByAuthor}
                className="w-full px-4 py-2 bg-teal-500 text-white rounded hover:bg-teal-600"
                disabled={loading}
              >
                著者の記事を取得
              </button>
            </div>
          </div>

          {/* タグ別記事取得 */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold mb-4">タグ別記事取得</h2>
            <div className="space-y-3">
              <input
                type="text"
                value={tagName}
                onChange={(e) => setTagName(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="タグ名"
              />
              <button
                onClick={handleGetArticlesByTag}
                className="w-full px-4 py-2 bg-teal-500 text-white rounded hover:bg-teal-600"
                disabled={loading}
              >
                タグの記事を取得
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
