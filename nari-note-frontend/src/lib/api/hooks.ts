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
export const useSignUp = (options?: Omit<UseMutationOptions<AuthResponse, Error, SignUpRequest>, 'mutationFn'>) => {
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: authApi.signUp,
    onSuccess: (...args) => {
      const [data] = args;
      if (typeof window !== 'undefined') {
        localStorage.setItem('authToken', data.token);
      }
      userOnSuccess?.(...args);
    },
  });
};

export const useSignIn = (options?: Omit<UseMutationOptions<AuthResponse, Error, SignInRequest>, 'mutationFn'>) => {
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: authApi.signIn,
    onSuccess: (...args) => {
      const [data] = args;
      if (typeof window !== 'undefined') {
        localStorage.setItem('authToken', data.token);
      }
      userOnSuccess?.(...args);
    },
  });
};

// Article Hooks
export const useCreateArticle = (options?: Omit<UseMutationOptions<CreateArticleResponse, Error, CreateArticleRequest>, 'mutationFn'>) => {
  const queryClient = useQueryClient();
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: articlesApi.create,
    onSuccess: (...args) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
      userOnSuccess?.(...args);
    },
  });
};

export const useArticle = (id: number, options?: Omit<UseQueryOptions<GetArticleResponse, Error>, 'queryKey' | 'queryFn'>) => {
  return useQuery({
    queryKey: queryKeys.articles.byId(id),
    queryFn: () => articlesApi.getById(id),
    enabled: id > 0,
    ...options,
  });
};

export const useUpdateArticle = (options?: Omit<UseMutationOptions<UpdateArticleResponse, Error, { id: number; data: UpdateArticleRequest }>, 'mutationFn'>) => {
  const queryClient = useQueryClient();
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: ({ id, data }) => articlesApi.update(id, data),
    onSuccess: (...args) => {
      const [, variables] = args;
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.byId(variables.id) });
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
      userOnSuccess?.(...args);
    },
  });
};

export const useDeleteArticle = (options?: Omit<UseMutationOptions<void, Error, { id: number; userId: number }>, 'mutationFn'>) => {
  const queryClient = useQueryClient();
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: ({ id, userId }) => articlesApi.delete(id, userId),
    onSuccess: (...args) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.all });
      userOnSuccess?.(...args);
    },
  });
};

export const useArticlesByAuthor = (authorId: number, options?: Omit<UseQueryOptions<GetArticlesByAuthorResponse, Error>, 'queryKey' | 'queryFn'>) => {
  return useQuery({
    queryKey: queryKeys.articles.byAuthor(authorId),
    queryFn: () => articlesApi.getByAuthor(authorId),
    enabled: authorId > 0,
    ...options,
  });
};

export const useArticlesByTag = (tagName: string, options?: Omit<UseQueryOptions<GetArticlesByTagResponse, Error>, 'queryKey' | 'queryFn'>) => {
  return useQuery({
    queryKey: queryKeys.articles.byTag(tagName),
    queryFn: () => articlesApi.getByTag(tagName),
    enabled: !!tagName,
    ...options,
  });
};

export const useToggleLike = (options?: Omit<UseMutationOptions<ToggleLikeResponse, Error, number>, 'mutationFn'>) => {
  const queryClient = useQueryClient();
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: articlesApi.toggleLike,
    onSuccess: (...args) => {
      const [, articleId] = args;
      queryClient.invalidateQueries({ queryKey: queryKeys.articles.byId(articleId) });
      userOnSuccess?.(...args);
    },
  });
};

// User Hooks
export const useUserProfile = (userId: number, options?: Omit<UseQueryOptions<GetUserProfileResponse, Error>, 'queryKey' | 'queryFn'>) => {
  return useQuery({
    queryKey: queryKeys.users.profile(userId),
    queryFn: () => usersApi.getProfile(userId),
    enabled: userId > 0,
    ...options,
  });
};

export const useUpdateUserProfile = (options?: Omit<UseMutationOptions<UpdateUserProfileResponse, Error, UpdateUserProfileRequest>, 'mutationFn'>) => {
  const queryClient = useQueryClient();
  const userOnSuccess = options?.onSuccess;
  return useMutation({
    ...options,
    mutationFn: usersApi.updateProfile,
    onSuccess: (...args) => {
      const [data] = args;
      queryClient.invalidateQueries({ queryKey: queryKeys.users.profile(data.id) });
      userOnSuccess?.(...args);
    },
  });
};

// Health Check Hook
export const useHealthCheck = (options?: Omit<UseQueryOptions<{ status: string }, Error>, 'queryKey' | 'queryFn'>) => {
  return useQuery({
    queryKey: queryKeys.health,
    queryFn: healthApi.check,
    ...options,
  });
};
