import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import type {
  ApiResponse,
  CursorPageResponse,
  MatchRequestDto,
  ConversationDto,
} from '../../../shared/types';

export function useMatchInbox(status?: string) {
  return useQuery({
    queryKey: ['matches', 'inbox', status],
    queryFn: async () => {
      const res = await api.get<ApiResponse<CursorPageResponse<MatchRequestDto>>>(
        '/matches/inbox',
        {
          params: { status, pageSize: 50 },
        }
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Sorğular yuklenmedi');
      }
      return res.data.data.items;
    },
    staleTime: 1000 * 60,
  });
}

export function useRespondToMatch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      matchId,
      action,
    }: {
      matchId: string;
      action: 'accept' | 'decline';
    }) => {
      const res = await api.put<ApiResponse<{ conversationId?: string }>>(
        `/matches/${matchId}/respond`,
        { action }
      );
      if (!res.data.success) {
        throw new Error(res.data.error?.message ?? 'Cavab verilmedi');
      }
      return res.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['matches'] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
  });
}

export function useConversations() {
  return useQuery({
    queryKey: ['conversations'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<ConversationDto[]>>(
        '/conversations'
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Sohbetler yuklenmedi');
      }
      return res.data.data;
    },
    staleTime: 1000 * 30,
    refetchInterval: 1000 * 60,
  });
}
