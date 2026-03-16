import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, CursorPageResponse, PostDto } from '../../../shared/types';

export function useVenueFeed(venueId: string) {
  return useInfiniteQuery({
    queryKey: ['venue', venueId, 'feed'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<PostDto>>>(
        `/venues/${venueId}/feed`,
        {
          params: { cursor: pageParam, pageSize: 20 },
        }
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Lent yuklenmedi');
      }
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => (last.hasMore ? last.nextCursor : undefined),
    staleTime: 1000 * 60 * 2,
    enabled: !!venueId,
  });
}

export function useToggleLike(venueId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (postId: string) => {
      const res = await api.post<ApiResponse<{ liked: boolean; likeCount: number }>>(
        `/posts/${postId}/like`
      );
      if (!res.data.success || !res.data.data) {
        throw new Error('Like etmek mumkun olmadi');
      }
      return res.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['venue', venueId, 'feed'] });
    },
  });
}
