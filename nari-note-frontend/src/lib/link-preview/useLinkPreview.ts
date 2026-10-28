import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api/client';
import type { LinkPreviewResponse } from './types';

export function useLinkPreview(url: string) {
  return useQuery<LinkPreviewResponse>({
    queryKey: ['linkPreview', url],
    queryFn: () => apiClient.get<LinkPreviewResponse>('/api/link-preview', { params: { url } }),
    staleTime: Infinity,
    retry: false,
  });
}
