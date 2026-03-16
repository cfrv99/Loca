import { useQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, PublicUserDto } from '../../../shared/types';

export function usePublicProfile(userId: string) {
  return useQuery({
    queryKey: ['user', userId, 'profile'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<PublicUserDto>>(
        `/users/${userId}`
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Profil tapilmadi');
      }
      return res.data.data;
    },
    enabled: !!userId,
    staleTime: 1000 * 60 * 5,
    retry: 2,
  });
}
