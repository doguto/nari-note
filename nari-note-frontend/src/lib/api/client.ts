import axios, { type AxiosResponse, type InternalAxiosRequestConfig, type AxiosRequestConfig } from 'axios';
import { unauthorizedHandler } from '@/lib/unauthorizedHandler';
import { forbiddenHandler } from '@/lib/forbiddenHandler';

const API_BASE_URL = '';

// APIクライアントの型定義を拡張
// インターセプターでresponse.dataを返すため、型を調整
const axiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// リクエストインターセプター
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    if (config.data instanceof FormData) {
      delete config.headers['Content-Type'];
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// レスポンスインターセプター（エラーハンドリング）
axiosInstance.interceptors.response.use(
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  <T = any>(response: AxiosResponse<T>): T => {
    return response.data;
  },
  (error) => {
    const serverMessage = error.response?.data?.message;
    if (error.response?.status === 401) {
      // 未認証: ログインモーダルを表示
      unauthorizedHandler.trigger();
    } else if (error.response?.status === 403) {
      // ログイン済みだが権限が無い: 権限エラーモーダルを表示
      forbiddenHandler.trigger(serverMessage);
    }
    return Promise.reject(serverMessage ? new Error(serverMessage) : error);
  }
);

// 型安全なAPIクライアント
export const apiClient = {
  get: <T>(url: string, config?: AxiosRequestConfig): Promise<T> => {
    return axiosInstance.get(url, config) as Promise<T>;
  },
  post: <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> => {
    return axiosInstance.post(url, data, config) as Promise<T>;
  },
  put: <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> => {
    return axiosInstance.put(url, data, config) as Promise<T>;
  },
  delete: <T>(url: string, config?: AxiosRequestConfig): Promise<T> => {
    return axiosInstance.delete(url, config) as Promise<T>;
  },
};
