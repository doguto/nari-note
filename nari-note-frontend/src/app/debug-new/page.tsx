'use client';

import { useState } from 'react';
import Link from 'next/link';
import {
  useSignUp,
  useSignIn,
  useCreateArticle,
  useArticle,
  useUpdateArticle,
  useDeleteArticle,
  useToggleLike,
  useUserProfile,
  useUpdateUserProfile,
  useArticlesByAuthor,
  useArticlesByTag,
  useHealthCheck,
} from '@/lib/api';

export default function NewDebugPage() {
  // State管理
  const [apiEndpoint] = useState('http://localhost:5243');
  const [authToken, setAuthToken] = useState<string>('');
  
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
  const [authorId, setAuthorId] = useState('');
  const [tagName, setTagName] = useState('');
  
  // User Profile
  const [userId, setUserId] = useState('');
  const [userName, setUserName] = useState('');
  const [userBio, setUserBio] = useState('');
  const [userProfileImage, setUserProfileImage] = useState('');

  // API Hooks
  const signUp = useSignUp({
    onSuccess: (data) => {
      setAuthToken(data.token);
      alert(`✅ サインアップ成功！\nユーザーID: ${data.id}\n名前: ${data.name}`);
    },
    onError: (error) => {
      alert(`❌ サインアップエラー\n${error.message}`);
    },
  });

  const signIn = useSignIn({
    onSuccess: (data) => {
      setAuthToken(data.token);
      alert(`✅ サインイン成功！\nユーザーID: ${data.id}\n名前: ${data.name}`);
    },
    onError: (error) => {
      alert(`❌ サインインエラー\n${error.message}`);
    },
  });

  const createArticle = useCreateArticle({
    onSuccess: (data) => {
      setArticleId(data.id.toString());
      alert(`✅ 記事作成成功！\n記事ID: ${data.id}`);
    },
    onError: (error) => {
      alert(`❌ 記事作成エラー\n${error.message}`);
    },
  });

  const { data: articleData, isLoading: articleLoading, refetch: refetchArticle } = useArticle(
    parseInt(articleId) || 0,
    { enabled: false }
  );

  const updateArticle = useUpdateArticle({
    onSuccess: (data) => {
      alert(`✅ 記事更新成功！\n記事ID: ${data.id}`);
    },
    onError: (error) => {
      alert(`❌ 記事更新エラー\n${error.message}`);
    },
  });

  const deleteArticle = useDeleteArticle({
    onSuccess: () => {
      alert('✅ 記事削除成功！');
      setArticleId('');
    },
    onError: (error) => {
      alert(`❌ 記事削除エラー\n${error.message}`);
    },
  });

  const toggleLike = useToggleLike({
    onSuccess: (data) => {
      alert(`✅ いいね切り替え成功！\nいいね数: ${data.likeCount}\nいいね状態: ${data.isLiked ? 'いいね済み' : '未いいね'}`);
    },
    onError: (error) => {
      alert(`❌ いいね切り替えエラー\n${error.message}`);
    },
  });

  const { data: userProfileData, isLoading: userProfileLoading, refetch: refetchUserProfile } = useUserProfile(
    parseInt(userId) || 0,
    { enabled: false }
  );

  const updateUserProfile = useUpdateUserProfile({
    onSuccess: (data) => {
      alert(`✅ プロフィール更新成功！\nユーザーID: ${data.id}`);
    },
    onError: (error) => {
      alert(`❌ プロフィール更新エラー\n${error.message}`);
    },
  });

  const { data: articlesByAuthorData, isLoading: articlesByAuthorLoading, refetch: refetchArticlesByAuthor } = useArticlesByAuthor(
    parseInt(authorId) || 0,
    { enabled: false }
  );

  const { data: articlesByTagData, isLoading: articlesByTagLoading, refetch: refetchArticlesByTag } = useArticlesByTag(
    tagName,
    { enabled: false }
  );

  const { data: healthData, isLoading: healthLoading, refetch: refetchHealth } = useHealthCheck({
    enabled: false,
  });

  // ハンドラー関数
  const handleSignUp = () => {
    signUp.mutate({
      email: signUpEmail,
      name: signUpUsername,
      password: signUpPassword,
    });
  };

  const handleSignIn = () => {
    signIn.mutate({
      usernameOrEmail: signInEmail,
      password: signInPassword,
    });
  };

  const handleCreateArticle = () => {
    const tags = articleTags.split(',').map(tag => tag.trim()).filter(tag => tag);
    createArticle.mutate({
      title: articleTitle,
      body: articleContent,
      authorId: parseInt(userId) || 1,
      tags: tags,
      isPublished: true,
    });
  };

  const handleGetArticle = () => {
    if (!articleId) {
      alert('❌ 記事IDを入力してください');
      return;
    }
    refetchArticle();
  };

  const handleUpdateArticle = () => {
    if (!articleId) {
      alert('❌ 記事IDを入力してください');
      return;
    }
    const tags = articleTags.split(',').map(tag => tag.trim()).filter(tag => tag);
    updateArticle.mutate({
      id: parseInt(articleId),
      data: {
        title: articleTitle,
        body: articleContent,
        tags: tags,
      },
    });
  };

  const handleDeleteArticle = () => {
    if (!articleId || !userId) {
      alert('❌ 記事IDとユーザーIDを入力してください');
      return;
    }
    deleteArticle.mutate({
      id: parseInt(articleId),
      userId: parseInt(userId),
    });
  };

  const handleToggleLike = () => {
    if (!articleId) {
      alert('❌ 記事IDを入力してください');
      return;
    }
    toggleLike.mutate(parseInt(articleId));
  };

  const handleGetUserProfile = () => {
    if (!userId) {
      alert('❌ ユーザーIDを入力してください');
      return;
    }
    refetchUserProfile();
  };

  const handleUpdateUserProfile = () => {
    const updates: Record<string, string> = {};
    if (userName) updates.name = userName;
    if (userBio) updates.bio = userBio;
    if (userProfileImage) updates.profileImage = userProfileImage;
    
    if (Object.keys(updates).length === 0) {
      alert('❌ 少なくとも1つのフィールドを入力してください');
      return;
    }
    
    updateUserProfile.mutate(updates);
  };

  const handleGetArticlesByAuthor = () => {
    if (!authorId) {
      alert('❌ 著者IDを入力してください');
      return;
    }
    refetchArticlesByAuthor();
  };

  const handleGetArticlesByTag = () => {
    if (!tagName) {
      alert('❌ タグ名を入力してください');
      return;
    }
    refetchArticlesByTag();
  };

  const handleHealthCheck = () => {
    refetchHealth();
  };

  const isLoading = signUp.isPending || signIn.isPending || createArticle.isPending || 
                    updateArticle.isPending || deleteArticle.isPending || toggleLike.isPending || 
                    updateUserProfile.isPending || articleLoading || userProfileLoading || 
                    articlesByAuthorLoading || articlesByTagLoading || healthLoading;

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-6xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-3xl font-bold">API Debug Console (TanStack Query版)</h1>
            <p className="text-sm text-gray-600 mt-2">TanStack QueryとTypeScriptを使った型安全なAPI通信</p>
          </div>
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
              className="flex-1 px-4 py-2 border rounded bg-gray-100"
              placeholder="http://localhost:5243"
              disabled
            />
            <button
              onClick={handleHealthCheck}
              className="px-6 py-2 bg-green-500 text-white rounded hover:bg-green-600 disabled:opacity-50"
              disabled={isLoading}
            >
              Health Check
            </button>
          </div>
          {healthData && (
            <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded">
              <pre>{JSON.stringify(healthData, null, 2)}</pre>
            </div>
          )}
        </div>

        {/* 認証トークン表示 */}
        {authToken && (
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-bold mb-4">認証トークン</h2>
            <div className="bg-gray-100 p-3 rounded font-mono text-sm break-all">
              {authToken}
            </div>
            <button
              onClick={() => {
                setAuthToken('');
                localStorage.removeItem('authToken');
              }}
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
                className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50"
                disabled={isLoading}
              >
                {signUp.isPending ? '処理中...' : 'サインアップ'}
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
                className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50"
                disabled={isLoading}
              >
                {signIn.isPending ? '処理中...' : 'サインイン'}
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
                className="w-full px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 disabled:opacity-50"
                disabled={isLoading || !authToken}
              >
                {createArticle.isPending ? '処理中...' : '記事を作成'}
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
                className="w-full px-4 py-2 bg-purple-500 text-white rounded hover:bg-purple-600 disabled:opacity-50"
                disabled={isLoading}
              >
                {articleLoading ? '処理中...' : '記事を取得'}
              </button>
              {articleData && (
                <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded">
                  <p className="font-bold">{articleData.title}</p>
                  <p className="text-sm text-gray-600">{articleData.body}</p>
                  <p className="text-xs text-gray-500 mt-2">いいね: {articleData.likeCount}</p>
                </div>
              )}
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
              <button
                onClick={handleUpdateArticle}
                className="w-full px-4 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600 disabled:opacity-50"
                disabled={isLoading || !authToken}
              >
                {updateArticle.isPending ? '処理中...' : '記事を更新'}
              </button>
              {!authToken && (
                <p className="text-sm text-red-500">※ 認証が必要です</p>
              )}
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
              <input
                type="text"
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                className="w-full px-4 py-2 border rounded"
                placeholder="ユーザーID"
              />
              <button
                onClick={handleDeleteArticle}
                className="w-full px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 disabled:opacity-50"
                disabled={isLoading || !authToken}
              >
                {deleteArticle.isPending ? '処理中...' : '記事を削除'}
              </button>
              {!authToken && (
                <p className="text-sm text-red-500">※ 認証が必要です</p>
              )}
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
                className="w-full px-4 py-2 bg-pink-500 text-white rounded hover:bg-pink-600 disabled:opacity-50"
                disabled={isLoading || !authToken}
              >
                {toggleLike.isPending ? '処理中...' : 'いいねを切り替え'}
              </button>
              {!authToken && (
                <p className="text-sm text-red-500">※ 認証が必要です</p>
              )}
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
                className="w-full px-4 py-2 bg-indigo-500 text-white rounded hover:bg-indigo-600 disabled:opacity-50"
                disabled={isLoading}
              >
                {userProfileLoading ? '処理中...' : 'プロフィールを取得'}
              </button>
              {userProfileData && (
                <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded">
                  <p className="font-bold">{userProfileData.name}</p>
                  <p className="text-sm text-gray-600">{userProfileData.email}</p>
                  <p className="text-xs text-gray-500">{userProfileData.bio}</p>
                </div>
              )}
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
                className="w-full px-4 py-2 bg-indigo-500 text-white rounded hover:bg-indigo-600 disabled:opacity-50"
                disabled={isLoading || !authToken}
              >
                {updateUserProfile.isPending ? '処理中...' : 'プロフィールを更新'}
              </button>
              {!authToken && (
                <p className="text-sm text-red-500">※ 認証が必要です</p>
              )}
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
                className="w-full px-4 py-2 bg-teal-500 text-white rounded hover:bg-teal-600 disabled:opacity-50"
                disabled={isLoading}
              >
                {articlesByAuthorLoading ? '処理中...' : '著者の記事を取得'}
              </button>
              {articlesByAuthorData && (
                <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded max-h-40 overflow-y-auto">
                  <p className="font-bold mb-2">記事数: {articlesByAuthorData.articles.length}</p>
                  {articlesByAuthorData.articles.map((article) => (
                    <div key={article.id} className="text-sm mb-1">
                      {article.id}: {article.title}
                    </div>
                  ))}
                </div>
              )}
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
                className="w-full px-4 py-2 bg-teal-500 text-white rounded hover:bg-teal-600 disabled:opacity-50"
                disabled={isLoading}
              >
                {articlesByTagLoading ? '処理中...' : 'タグの記事を取得'}
              </button>
              {articlesByTagData && (
                <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded max-h-40 overflow-y-auto">
                  <p className="font-bold mb-2">記事数: {articlesByTagData.articles.length}</p>
                  {articlesByTagData.articles.map((article) => (
                    <div key={article.id} className="text-sm mb-1">
                      {article.id}: {article.title}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
