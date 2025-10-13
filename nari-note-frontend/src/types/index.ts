export interface User {
  id: string;
  username: string;
  displayName: string;
  email: string;
  bio?: string;
  avatarUrl?: string;
  followingCount: number;
  followersCount: number;
  createdAt: string;
}

export interface Article {
  id: string;
  title: string;
  content: string;
  authorId: string;
  author: User;
  tags: Tag[];
  likes: number;
  isLiked: boolean;
  commentCount: number;
  isDraft: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Tag {
  id: string;
  name: string;
  description?: string;
  articleCount: number;
  followersCount: number;
}

export interface Notification {
  id: string;
  type: 'article' | 'like' | 'comment' | 'follow';
  userId: string;
  user: User;
  articleId?: string;
  article?: Article;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface Draft extends Article {
  isDraft: true;
}
