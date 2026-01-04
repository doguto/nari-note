import { apiClient } from './client';
import type {
  SignUpRequest,
  SignInRequest,
  AuthResponse,
  CreateArticleRequest,
  CreateArticleResponse,
  UpdateArticleRequest,
  UpdateArticleResponse,
  GetArticleResponse,
  GetArticlesByAuthorResponse,
  GetArticlesByTagResponse,
  ToggleLikeResponse,
  GetUserProfileResponse,
  UpdateUserProfileRequest,
  UpdateUserProfileResponse,
} from './types';

// Auth API
export const authApi = {
  signUp: async (data: SignUpRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/api/auth/signup', data);
    return response.data;
  },

  signIn: async (data: SignInRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/api/auth/signin', data);
    return response.data;
  },
};

// Articles API
export const articlesApi = {
  create: async (data: CreateArticleRequest): Promise<CreateArticleResponse> => {
    const response = await apiClient.post<CreateArticleResponse>('/api/articles', data);
    return response.data;
  },

  getById: async (id: number): Promise<GetArticleResponse> => {
    const response = await apiClient.get<GetArticleResponse>(`/api/articles/${id}`);
    return response.data;
  },

  update: async (id: number, data: UpdateArticleRequest): Promise<UpdateArticleResponse> => {
    const response = await apiClient.put<UpdateArticleResponse>(`/api/articles/${id}`, data);
    return response.data;
  },

  delete: async (id: number, userId: number): Promise<void> => {
    await apiClient.delete(`/api/articles/${id}?userId=${userId}`);
  },

  getByAuthor: async (authorId: number): Promise<GetArticlesByAuthorResponse> => {
    const response = await apiClient.get<GetArticlesByAuthorResponse>(`/api/articles/author/${authorId}`);
    return response.data;
  },

  getByTag: async (tagName: string): Promise<GetArticlesByTagResponse> => {
    const response = await apiClient.get<GetArticlesByTagResponse>(`/api/articles/tag/${tagName}`);
    return response.data;
  },

  toggleLike: async (articleId: number): Promise<ToggleLikeResponse> => {
    const response = await apiClient.post<ToggleLikeResponse>(`/api/articles/${articleId}/like`);
    return response.data;
  },
};

// Users API
export const usersApi = {
  getProfile: async (userId: number): Promise<GetUserProfileResponse> => {
    const response = await apiClient.get<GetUserProfileResponse>(`/api/users/${userId}`);
    return response.data;
  },

  updateProfile: async (data: UpdateUserProfileRequest): Promise<UpdateUserProfileResponse> => {
    const response = await apiClient.put<UpdateUserProfileResponse>('/api/users', data);
    return response.data;
  },
};

// Health Check API
export const healthApi = {
  check: async (): Promise<{ status: string }> => {
    const response = await apiClient.get<{ status: string }>('/api/health');
    return response.data;
  },
};
