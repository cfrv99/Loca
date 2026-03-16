import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { api } from '@/shared/services/api-client';
import type { ApiResponse, CursorPageResponse, VenueCardDto, VenueDetailDto } from '@/shared/types';

export function useVenuesNearby() {
  return useInfiniteQuery({
    queryKey: ['venues', 'nearby'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<VenueCardDto>>>('/venues/nearby', {
        params: { cursor: pageParam, pageSize: 20, lat: 40.4093, lng: 49.8671 },
      });
      return res.data.data!;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => (lastPage.hasMore ? lastPage.nextCursor : undefined),
    staleTime: 1000 * 60 * 2,
    retry: 2,
  });
}

export function useVenueDetail(venueId: string) {
  return useQuery({
    queryKey: ['venues', venueId],
    queryFn: async () => {
      const res = await api.get<ApiResponse<VenueDetailDto>>(`/venues/${venueId}`);
      return res.data.data!;
    },
    staleTime: 1000 * 60 * 5,
    enabled: !!venueId,
  });
}
