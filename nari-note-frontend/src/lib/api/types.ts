// Auth types
export interface SignUpRequest {
  email: string;
  name: string;
  password: string;
}

export interface SignInRequest {
  usernameOrEmail: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  id: number;
  name: string;
  email: string;
}

// Article types
export interface CreateArticleRequest {
  title: string;
  body: string;
  authorId: number;
  tags: string[];
  isPublished?: boolean;
}

export interface UpdateArticleRequest {
  id?: number;
  title: string;
  body: string;
  tags: string[];
  isPublished?: boolean;
}

export interface CreateArticleResponse {
  id: number;
  createdAt: string;
}

export interface UpdateArticleResponse {
  id: number;
  updatedAt: string;
}

export interface GetArticleRequest {
  id: number;
}

export interface GetArticleResponse {
  id: number;
  title: string;
  body: string;
  authorId: number;
  authorName: string;
  tags: string[];
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  likeCount: number;
}

export interface GetArticlesByAuthorRequest {
  authorId: number;
}

export interface GetArticlesByAuthorResponse {
  articles: GetArticleResponse[];
}

export interface GetArticlesByTagRequest {
  tagName: string;
}

export interface GetArticlesByTagResponse {
  articles: GetArticleResponse[];
}

export interface DeleteArticleRequest {
  id: number;
  userId: number;
}

export interface ToggleLikeRequest {
  articleId: number;
}

export interface ToggleLikeResponse {
  isLiked: boolean;
  likeCount: number;
}

// User types
export interface GetUserProfileRequest {
  id: number;
}

export interface GetUserProfileResponse {
  id: number;
  name: string;
  email: string;
  bio?: string;
  profileImage?: string;
  createdAt: string;
}

export interface UpdateUserProfileRequest {
  name?: string;
  bio?: string;
  profileImage?: string;
}

export interface UpdateUserProfileResponse {
  id: number;
  name: string;
  bio?: string;
  profileImage?: string;
  updatedAt: string;
}

// Error types
export interface ErrorResponse {
  message: string;
  statusCode?: number;
}
