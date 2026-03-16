import { useState, useEffect, useCallback, useRef } from 'react';
import { useInfiniteQuery } from '@tanstack/react-query';
import { useSignalR } from '../../../shared/hooks/use-signalr';
import { api } from '../../../shared/services/api-client';
import { CONFIG } from '../../../shared/constants/config';
import type {
  ApiResponse,
  CursorPageResponse,
  ChatMessageDto,
  ActiveUserDto,
  ReactionDto,
  GiftEventDto,
  VenueCountDto,
} from '../../../shared/types';

export function useVenueChat(venueId: string) {
  const { connection, isConnected } = useSignalR(CONFIG.SIGNALR_CHAT_HUB);
  const [newMessages, setNewMessages] = useState<ChatMessageDto[]>([]);
  const [typingUsers, setTypingUsers] = useState<Map<string, string>>(new Map());
  const [activeUsers, setActiveUsers] = useState<VenueCountDto>({ total: 0, male: 0, female: 0 });
  const [pendingGift, setPendingGift] = useState<GiftEventDto | null>(null);
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const lastTypingSentRef = useRef<number>(0);

  // Load chat history with cursor pagination
  const history = useInfiniteQuery({
    queryKey: ['chat', venueId, 'history'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<ChatMessageDto>>>(
        `/venues/${venueId}/messages`,
        { params: { cursor: pageParam, pageSize: 50 } },
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Mesajlar yuklenmedi');
      }
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => (last.hasMore ? last.nextCursor : undefined),
    staleTime: Infinity,
  });

  // Join venue + listen for real-time events
  useEffect(() => {
    if (!connection || !isConnected) return;

    connection.invoke('JoinVenue', venueId).catch((err) => {
      if (__DEV__) console.error('JoinVenue failed:', err);
    });

    const onReceiveMessage = (msg: ChatMessageDto) => {
      setNewMessages((prev) => [...prev, msg]);
    };

    const onUserJoined = (_user: ActiveUserDto) => {
      // User joined is also reflected via activeUsersUpdated
    };

    const onUserLeft = (_userId: string) => {
      // User left is also reflected via activeUsersUpdated
    };

    const onTypingStarted = (userId: string, displayName: string) => {
      setTypingUsers((prev) => {
        const next = new Map(prev);
        next.set(userId, displayName);
        return next;
      });
    };

    const onTypingStopped = (userId: string) => {
      setTypingUsers((prev) => {
        const next = new Map(prev);
        next.delete(userId);
        return next;
      });
    };

    const onReactionUpdated = (_messageId: string, _reactions: ReactionDto[]) => {
      // Reactions are per-message; update in-place if needed
    };

    const onGiftReceived = (gift: GiftEventDto) => {
      setPendingGift(gift);
    };

    const onActiveUsersUpdated = (count: VenueCountDto) => {
      setActiveUsers(count);
    };

    connection.on('receiveMessage', onReceiveMessage);
    connection.on('userJoined', onUserJoined);
    connection.on('userLeft', onUserLeft);
    connection.on('typingStarted', onTypingStarted);
    connection.on('typingStopped', onTypingStopped);
    connection.on('reactionUpdated', onReactionUpdated);
    connection.on('giftReceived', onGiftReceived);
    connection.on('activeUsersUpdated', onActiveUsersUpdated);

    return () => {
      connection.invoke('LeaveVenue', venueId).catch(() => {});
      connection.off('receiveMessage', onReceiveMessage);
      connection.off('userJoined', onUserJoined);
      connection.off('userLeft', onUserLeft);
      connection.off('typingStarted', onTypingStarted);
      connection.off('typingStopped', onTypingStopped);
      connection.off('reactionUpdated', onReactionUpdated);
      connection.off('giftReceived', onGiftReceived);
      connection.off('activeUsersUpdated', onActiveUsersUpdated);
    };
  }, [connection, isConnected, venueId]);

  const sendMessage = useCallback(
    async (content: string, type = 'text', replyToId?: string, metadata?: Record<string, unknown>) => {
      if (!connection || !isConnected) return;
      await connection.invoke('SendMessage', venueId, content, type, replyToId ?? null, metadata ?? null);
    },
    [connection, isConnected, venueId],
  );

  const sendReaction = useCallback(
    async (messageId: string, emoji: string) => {
      if (!connection || !isConnected) return;
      await connection.invoke('SendReaction', messageId, emoji);
    },
    [connection, isConnected],
  );

  const startTyping = useCallback(() => {
    if (!connection || !isConnected) return;
    const now = Date.now();
    // Debounce: only send typing every 3 seconds
    if (now - lastTypingSentRef.current < 3000) return;
    lastTypingSentRef.current = now;
    connection.invoke('StartTyping', venueId).catch(() => {});

    // Auto-stop after 3s of no further calls
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    typingTimeoutRef.current = setTimeout(() => {
      connection.invoke('StopTyping', venueId).catch(() => {});
    }, 3000);
  }, [connection, isConnected, venueId]);

  const stopTyping = useCallback(() => {
    if (!connection || !isConnected) return;
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    connection.invoke('StopTyping', venueId).catch(() => {});
    lastTypingSentRef.current = 0;
  }, [connection, isConnected, venueId]);

  const clearPendingGift = useCallback(() => {
    setPendingGift(null);
  }, []);

  // Merge history + new real-time messages
  const historyMessages = history.data?.pages.flatMap((p) => p.items) ?? [];
  const allMessages = [...historyMessages, ...newMessages];

  return {
    messages: allMessages,
    sendMessage,
    sendReaction,
    typingUsers,
    activeUsers,
    isConnected,
    pendingGift,
    clearPendingGift,
    startTyping,
    stopTyping,
    fetchNextPage: history.fetchNextPage,
    hasNextPage: history.hasNextPage,
    isLoadingHistory: history.isLoading,
    historyError: history.error,
  };
}
