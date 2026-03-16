import { useInfiniteQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, CursorPageResponse, ActiveUserDto } from '../../../shared/types';

export function useVenuePeople(venueId: string) {
  return useInfiniteQuery({
    queryKey: ['venue', venueId, 'people'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<ActiveUserDto>>>(
        `/venues/${venueId}/people`,
        {
          params: { cursor: pageParam, pageSize: 20 },
        }
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Insanlar yuklenmedi');
      }
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => (last.hasMore ? last.nextCursor : undefined),
    staleTime: 1000 * 60,
    enabled: !!venueId,
  });
}
