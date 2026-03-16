import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type {
  ApiResponse,
  BalanceDto,
  GiftDto,
  GiftSendRequest,
  GiftSendResult,
  CoinPackageDto,
} from '../../../shared/types';

export function useBalance() {
  return useQuery({
    queryKey: ['economy', 'balance'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<BalanceDto>>('/economy/balance');
      if (!res.data.success || !res.data.data) {
        throw new Error('Balans yuklenmedi');
      }
      return res.data.data;
    },
    staleTime: 1000 * 30,
  });
}

export function useGiftCatalog() {
  return useQuery({
    queryKey: ['economy', 'gifts'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<GiftDto[]>>('/economy/gifts');
      if (!res.data.success || !res.data.data) {
        throw new Error('Hediyyeler yuklenmedi');
      }
      return res.data.data;
    },
    staleTime: 1000 * 60 * 10,
  });
}

export function useCoinPackages() {
  return useQuery({
    queryKey: ['economy', 'packages'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<CoinPackageDto[]>>(
        '/economy/packages'
      );
      if (!res.data.success || !res.data.data) {
        throw new Error('Paketler yuklenmedi');
      }
      return res.data.data;
    },
    staleTime: 1000 * 60 * 60,
  });
}

export function useSendGift() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: GiftSendRequest) => {
      const res = await api.post<ApiResponse<GiftSendResult>>(
        '/economy/gifts/send',
        request
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Hediyye gonderilmedi');
      }
      return res.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['economy', 'balance'] });
    },
  });
}
