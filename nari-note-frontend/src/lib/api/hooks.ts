import { useMutation, useQuery, useQueryClient, type UseMutationOptions, type UseQueryOptions } from '@tanstack/react-query';
import { authApi, articlesApi, usersApi, healthApi } from './endpoints';
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

// Query Keys
export const queryKeys = {
  auth: ['auth'] as const,
  articles: {
    all: ['articles'] as const,
    byId: (id: number) => ['articles', id] as const,
    byAuthor: (authorId: number) => ['articles', 'author', authorId] as const,
    byTag: (tagName: string) => ['articles', 'tag', tagName] as const,
  },
  users: {
    profile: (userId: number) => ['users', userId] as const,
  },
  health: ['health'] as const,
};

// Auth Hooks
export const useSignUp = (options?: UseMutationOptions<AuthResponse, Error, SignUpRequest>) => {
  return useMutation({
    mutationFn: authApi.signUp,
    onSuccess: (data) => {
      if (typeof window !== 'undefined') {
        localStorage.setItem('authToken', data.token);
      }
    },
    ...options,
  });
};

export const useSignIn = (options?: UseMutationOptions<AuthResponse, Error, SignInRequest>) => {
  return useMutation({
    mutationFn: authApi.signIn,
    onSuccess: (data) => {
      if (typeof window !== 'undefined') {
        localStorage.setItem('authToken', data.token);
      }
    },
    ...options,
  });
};

// Article Hooks
export const useCreateArticle = (options?: UseMutationOptions<CreateArticleResponse, Error, CreateArticleRequest>) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: articlesApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
    },
    ...options,
  });
};

export const useArticle = (id: number, options?: UseQueryOptions<GetArticleResponse, Error>) => {
  return useQuery({
    queryKey: queryKeys.articles.byId(id),
    queryFn: () => articlesApi.getById(id),
    enabled: id > 0,
    ...options,
  });
};

export const useUpdateArticle = (options?: UseMutationOptions<UpdateArticleResponse, Error, { id: number; data: UpdateArticleRequest }>) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => articlesApi.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.byId(variables.id) });
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
    },
    ...options,
  });
};

export const useDeleteArticle = (options?: UseMutationOptions<void, Error, { id: number; userId: number }>) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, userId }) => articlesApi.delete(id, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
    },
    ...options,
  });
};

export const useArticlesByAuthor = (authorId: number, options?: UseQueryOptions<GetArticlesByAuthorResponse, Error>) => {
  return useQuery({
    queryKey: queryKeys.articles.byAuthor(authorId),
    queryFn: () => articlesApi.getByAuthor(authorId),
    enabled: authorId > 0,
    ...options,
  });
};

export const useArticlesByTag = (tagName: string, options?: UseQueryOptions<GetArticlesByTagResponse, Error>) => {
  return useQuery({
    queryKey: queryKeys.articles.byTag(tagName),
    queryFn: () => articlesApi.getByTag(tagName),
    enabled: !!tagName,
    ...options,
  });
};

export const useToggleLike = (options?: UseMutationOptions<ToggleLikeResponse, Error, number>) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: articlesApi.toggleLike,
    onSuccess: (_, articleId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.byId(articleId) });
    },
    ...options,
  });
};

// User Hooks
export const useUserProfile = (userId: number, options?: UseQueryOptions<GetUserProfileResponse, Error>) => {
  return useQuery({
    queryKey: queryKeys.users.profile(userId),
    queryFn: () => usersApi.getProfile(userId),
    enabled: userId > 0,
    ...options,
  });
};

export const useUpdateUserProfile = (options?: UseMutationOptions<UpdateUserProfileResponse, Error, UpdateUserProfileRequest>) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: usersApi.updateProfile,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.profile(data.id) });
    },
    ...options,
  });
};

// Health Check Hook
export const useHealthCheck = (options?: UseQueryOptions<{ status: string }, Error>) => {
  return useQuery({
    queryKey: queryKeys.health,
    queryFn: healthApi.check,
    ...options,
  });
};
