import { useQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, GameSessionDto } from '../../../shared/types';

export function useVenueGames(venueId: string) {
  return useQuery({
    queryKey: ['venue', venueId, 'games'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<GameSessionDto[]>>(
        `/venues/${venueId}/games`
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Oyunlar yuklenmedi');
      }
      return res.data.data;
    },
    enabled: !!venueId,
    staleTime: 1000 * 30,
    refetchInterval: 1000 * 30,
  });
}
