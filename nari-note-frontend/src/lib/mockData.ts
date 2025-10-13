import { User, Article, Tag, Notification } from '@/types';

// モックユーザーデータ
export const mockUsers: User[] = [
  {
    id: '1',
    username: 'shogi_master',
    displayName: '将棋マスター',
    email: 'master@example.com',
    bio: '将棋歴15年。棋力は二段です。居飛車党で矢倉が得意です。',
    avatarUrl: '/avatars/user1.png',
    followingCount: 42,
    followersCount: 128,
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: '2',
    username: 'kakoi_lover',
    displayName: '囲い研究家',
    email: 'kakoi@example.com',
    bio: '各種囲いの研究をしています。',
    avatarUrl: '/avatars/user2.png',
    followingCount: 25,
    followersCount: 89,
    createdAt: '2024-02-20T14:30:00Z',
  },
  {
    id: '3',
    username: 'kifu_analyzer',
    displayName: '棋譜解析bot',
    email: 'kifu@example.com',
    bio: 'プロの棋譜を分析して解説記事を書いています。',
    followingCount: 10,
    followersCount: 256,
    createdAt: '2024-03-05T09:15:00Z',
  },
];

// モックタグデータ
export const mockTags: Tag[] = [
  { id: '1', name: '定跡', description: '将棋の定跡に関する記事', articleCount: 45, followersCount: 230 },
  { id: '2', name: '囲い', description: '各種囲いの解説', articleCount: 32, followersCount: 180 },
  { id: '3', name: '詰将棋', description: '詰将棋の問題と解説', articleCount: 28, followersCount: 150 },
  { id: '4', name: '戦法', description: '様々な戦法の紹介', articleCount: 56, followersCount: 310 },
  { id: '5', name: '棋譜解説', description: 'プロ棋士の対局解説', articleCount: 67, followersCount: 420 },
  { id: '6', name: '初心者向け', description: '将棋初心者のための記事', articleCount: 89, followersCount: 540 },
];

// モック記事データ
export const mockArticles: Article[] = [
  {
    id: '1',
    title: '矢倉囲いの基本形とその変化',
    content: `# 矢倉囲いの基本形

矢倉囲いは**居飛車戦法**の代表的な囲いです。

## 基本形の特徴

- 金銀三枚で固める
- 横からの攻めに強い
- 上部が薄い

> 矢倉囲いは「攻めは飛車角銀桂、守りは金銀三枚」と言われています。

## 組み方の手順

1. **金を上げる** - まずは７八金を７七金に上がります
2. **銀を繰り出す** - 次に３九銀を３八銀に繰り出します
3. **角道を止める** - 最後に７七銀で角道を止めます

### 完成形の盤面

\`\`\`
後手の持駒：なし
  ９ ８ ７ ６ ５ ４ ３ ２ １
+---------------------------+
|v香v桂v銀v金v玉v金v銀v桂v香|一
| ・v飛 ・ ・ ・ ・ ・v角 ・|二
|v歩v歩v歩v歩v歩v歩v歩v歩v歩|三
+---------------------------+
先手の持駒：なし
\`\`\`

この形から様々な変化があります。

---

**参考文献**: [日本将棋連盟](https://www.shogi.or.jp/)`,
    authorId: '1',
    author: mockUsers[0],
    tags: [mockTags[0], mockTags[1]],
    likes: 45,
    isLiked: false,
    commentCount: 12,
    isDraft: false,
    createdAt: '2025-01-10T10:30:00Z',
    updatedAt: '2025-01-10T10:30:00Z',
  },
  {
    id: '2',
    title: '三間飛車の基本的な指し方',
    content: `# 三間飛車入門

三間飛車は振り飛車の中でも攻撃的な戦法です。

## 特徴
- 攻めが早い
- 捌きを重視
- 居飛車穴熊に対抗しやすい

## 基本的な駒組み
飛車を3筋に振り、美濃囲いに組むのが基本です。`,
    authorId: '2',
    author: mockUsers[1],
    tags: [mockTags[3], mockTags[5]],
    likes: 32,
    isLiked: true,
    commentCount: 8,
    isDraft: false,
    createdAt: '2025-01-09T14:20:00Z',
    updatedAt: '2025-01-09T14:20:00Z',
  },
  {
    id: '3',
    title: '【初心者向け】駒の効率的な使い方',
    content: `# 駒の効率的な使い方

将棋で勝つためには、駒を効率よく使うことが大切です。

## 駒の価値
- 歩：1
- 香・桂：3-4
- 銀：5
- 金：6
- 角・飛：8-9

## 効率の良い使い方
1. 遊び駒を作らない
2. 複数の駒で連携する
3. 駒得を目指す`,
    authorId: '3',
    author: mockUsers[2],
    tags: [mockTags[5]],
    likes: 67,
    isLiked: false,
    commentCount: 23,
    isDraft: false,
    createdAt: '2025-01-08T09:15:00Z',
    updatedAt: '2025-01-08T09:15:00Z',
  },
  {
    id: '4',
    title: '詰将棋の解き方のコツ',
    content: `# 詰将棋攻略法

詰将棋を解くための基本的なコツを紹介します。

## 基本原則
1. 王手は最大限に
2. 駒は近い方から
3. 捨て駒を活用する

練習問題も用意していますので、挑戦してみてください。`,
    authorId: '1',
    author: mockUsers[0],
    tags: [mockTags[2], mockTags[5]],
    likes: 89,
    isLiked: true,
    commentCount: 34,
    isDraft: false,
    createdAt: '2025-01-07T16:45:00Z',
    updatedAt: '2025-01-07T16:45:00Z',
  },
  {
    id: '5',
    title: '藤井聡太の最新対局解説',
    content: `# 藤井聡太 vs 羽生善治 棋譜解説

先日行われた注目の一局を解説します。

## 序盤
角換わりの出だしから、藤井竜王が新手を披露。

## 中盤
激しい攻め合いとなり、一進一退の攻防が続きます。

## 終盤
藤井竜王の鋭い寄せが決まり、見事勝利を収めました。`,
    authorId: '3',
    author: mockUsers[2],
    tags: [mockTags[4]],
    likes: 234,
    isLiked: false,
    commentCount: 56,
    isDraft: false,
    createdAt: '2025-01-06T11:00:00Z',
    updatedAt: '2025-01-06T11:00:00Z',
  },
];

// モック下書きデータ
export const mockDrafts: Article[] = [
  {
    id: 'd1',
    title: '角換わりの新定跡研究（未完成）',
    content: '角換わりの最新定跡について研究中です。まだ途中ですが...',
    authorId: '1',
    author: mockUsers[0],
    tags: [mockTags[0]],
    likes: 0,
    isLiked: false,
    commentCount: 0,
    isDraft: true,
    createdAt: '2025-01-11T08:00:00Z',
    updatedAt: '2025-01-12T15:30:00Z',
  },
  {
    id: 'd2',
    title: '穴熊の崩し方',
    content: '穴熊囲いを攻略する方法をまとめています。',
    authorId: '1',
    author: mockUsers[0],
    tags: [mockTags[1]],
    likes: 0,
    isLiked: false,
    commentCount: 0,
    isDraft: true,
    createdAt: '2025-01-09T12:00:00Z',
    updatedAt: '2025-01-11T09:20:00Z',
  },
];

// モック通知データ
export const mockNotifications: Notification[] = [
  {
    id: 'n1',
    type: 'article',
    userId: '2',
    user: mockUsers[1],
    articleId: '2',
    article: mockArticles[1],
    message: '囲い研究家さんが新しい記事を投稿しました',
    isRead: false,
    createdAt: '2025-01-12T10:30:00Z',
  },
  {
    id: 'n2',
    type: 'like',
    userId: '3',
    user: mockUsers[2],
    articleId: '1',
    article: mockArticles[0],
    message: '棋譜解析botさんがあなたの記事にいいねしました',
    isRead: false,
    createdAt: '2025-01-12T09:15:00Z',
  },
  {
    id: 'n3',
    type: 'follow',
    userId: '3',
    user: mockUsers[2],
    message: '棋譜解析botさんがあなたをフォローしました',
    isRead: true,
    createdAt: '2025-01-11T14:20:00Z',
  },
  {
    id: 'n4',
    type: 'article',
    userId: '3',
    user: mockUsers[2],
    articleId: '5',
    article: mockArticles[4],
    message: '棋譜解析botさんが新しい記事を投稿しました',
    isRead: true,
    createdAt: '2025-01-10T11:00:00Z',
  },
];

// 現在のログインユーザー（モック）
export const currentUser: User = mockUsers[0];
