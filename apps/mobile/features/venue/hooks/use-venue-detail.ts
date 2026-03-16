import { useQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, VenueDetailDto } from '../../../shared/types';

export function useVenueDetail(venueId: string) {
  return useQuery({
    queryKey: ['venue', venueId],
    queryFn: async () => {
      const res = await api.get<ApiResponse<VenueDetailDto>>(
        `/venues/${venueId}`
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Məkan tapılmadı');
      }
      return res.data.data;
    },
    enabled: !!venueId,
    staleTime: 1000 * 60 * 5,
    retry: 2,
  });
}
