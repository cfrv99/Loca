import { useInfiniteQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, CursorPageResponse, VenueCardDto } from '../../../shared/types';

export function useVenuesNearby() {
  return useInfiniteQuery({
    queryKey: ['venues', 'nearby'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<VenueCardDto>>>('/venues/nearby', {
        params: {
          lat: 40.4093, // Baku default - in prod: get from expo-location
          lng: 49.8671,
          cursor: pageParam,
          pageSize: 20,
        },
      });
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Failed to load venues');
      }
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: 1000 * 60 * 2,
    retry: 2,
  });
}
