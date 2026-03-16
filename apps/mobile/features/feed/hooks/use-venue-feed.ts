import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, CursorPageResponse, PostDto, CommentDto } from '../../../shared/types';

export function useVenueFeed(venueId: string) {
  const queryClient = useQueryClient();

  const feed = useInfiniteQuery({
    queryKey: ['feed', venueId],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<PostDto>>>(
        `/venues/${venueId}/feed`,
        { params: { cursor: pageParam, pageSize: 20 } },
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Feed yuklenmədi');
      }
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => (last.hasMore ? last.nextCursor : undefined),
    staleTime: 1000 * 60 * 2,
  });

  const toggleLike = useMutation({
    mutationFn: async (postId: string) => {
      const res = await api.post<ApiResponse<{ liked: boolean; likeCount: number }>>(
        `/posts/${postId}/like`,
      );
      if (!res.data.success || !res.data.data) throw new Error('Like uğursuz oldu');
      return res.data.data;
    },
    onMutate: async (postId) => {
      // Optimistic update
      await queryClient.cancelQueries({ queryKey: ['feed', venueId] });
      const previous = queryClient.getQueryData(['feed', venueId]);

      queryClient.setQueryData(['feed', venueId], (old: typeof feed.data) => {
        if (!old) return old;
        return {
          ...old,
          pages: old.pages.map((page) => ({
            ...page,
            items: page.items.map((post) =>
              post.id === postId
                ? {
                    ...post,
                    isLikedByMe: !post.isLikedByMe,
                    likeCount: post.isLikedByMe
                      ? post.likeCount - 1
                      : post.likeCount + 1,
                  }
                : post,
            ),
          })),
        };
      });
      return { previous };
    },
    onError: (_err, _postId, context) => {
      if (context?.previous) {
        queryClient.setQueryData(['feed', venueId], context.previous);
      }
    },
  });

  const loadComments = async (postId: string, cursor?: string) => {
    const res = await api.get<ApiResponse<CursorPageResponse<CommentDto>>>(
      `/posts/${postId}/comments`,
      { params: { cursor, pageSize: 20 } },
    );
    if (!res.data.success || !res.data.data) throw new Error('Şərhlər yuklenmədi');
    return res.data.data;
  };

  const addComment = useMutation({
    mutationFn: async ({ postId, content }: { postId: string; content: string }) => {
      const res = await api.post<ApiResponse<CommentDto>>(
        `/posts/${postId}/comments`,
        { content },
      );
      if (!res.data.success || !res.data.data) throw new Error('Şərh göndərilmədi');
      return res.data.data;
    },
  });

  return {
    ...feed,
    toggleLike,
    loadComments,
    addComment,
  };
}
