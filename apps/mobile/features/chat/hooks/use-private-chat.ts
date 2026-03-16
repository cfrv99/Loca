import { useState, useEffect, useCallback, useRef } from 'react';
import { useInfiniteQuery } from '@tanstack/react-query';
import { useSignalR } from '../../../shared/hooks/use-signalr';
import { api } from '../../../shared/services/api-client';
import { CONFIG } from '../../../shared/constants/config';
import type {
  ApiResponse,
  CursorPageResponse,
  PrivateMessageDto,
  GiftEventDto,
} from '../../../shared/types';

interface OnlineStatus {
  isOnline: boolean;
  lastSeenAt?: string;
}

export function usePrivateChat(conversationId: string) {
  const { connection, isConnected } = useSignalR(CONFIG.SIGNALR_PRIVATE_CHAT_HUB);
  const [newMessages, setNewMessages] = useState<PrivateMessageDto[]>([]);
  const [typingUserId, setTypingUserId] = useState<string | null>(null);
  const [otherUserOnline, setOtherUserOnline] = useState<OnlineStatus>({ isOnline: false });
  const [pendingGift, setPendingGift] = useState<GiftEventDto | null>(null);
  const [lastReadMessageId, setLastReadMessageId] = useState<string | null>(null);
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const lastTypingSentRef = useRef<number>(0);

  // Load message history
  const history = useInfiniteQuery({
    queryKey: ['private-chat', conversationId, 'history'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<ApiResponse<CursorPageResponse<PrivateMessageDto>>>(
        `/conversations/${conversationId}/messages`,
        { params: { cursor: pageParam, pageSize: 50 } },
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.error?.message ?? 'Mesajlar yuklenmədi');
      }
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => (last.hasMore ? last.nextCursor : undefined),
    staleTime: Infinity,
  });

  // Listen for real-time events
  useEffect(() => {
    if (!connection || !isConnected) return;

    const onReceivePrivateMessage = (msg: PrivateMessageDto) => {
      if (msg.conversationId === conversationId) {
        setNewMessages((prev) => [...prev, msg]);
      }
    };

    const onTypingStarted = (convId: string, userId: string) => {
      if (convId === conversationId) {
        setTypingUserId(userId);
      }
    };

    const onTypingStopped = (convId: string, _userId: string) => {
      if (convId === conversationId) {
        setTypingUserId(null);
      }
    };

    const onMessagesRead = (convId: string, _readByUserId: string, upToMessageId: string) => {
      if (convId === conversationId) {
        setLastReadMessageId(upToMessageId);
      }
    };

    const onUserOnlineStatusChanged = (_userId: string, isOnline: boolean, lastSeenAt?: string) => {
      setOtherUserOnline({ isOnline, lastSeenAt });
    };

    const onGiftReceived = (gift: GiftEventDto) => {
      setPendingGift(gift);
    };

    connection.on('receivePrivateMessage', onReceivePrivateMessage);
    connection.on('typingStarted', onTypingStarted);
    connection.on('typingStopped', onTypingStopped);
    connection.on('messagesRead', onMessagesRead);
    connection.on('userOnlineStatusChanged', onUserOnlineStatusChanged);
    connection.on('giftReceived', onGiftReceived);

    return () => {
      connection.off('receivePrivateMessage', onReceivePrivateMessage);
      connection.off('typingStarted', onTypingStarted);
      connection.off('typingStopped', onTypingStopped);
      connection.off('messagesRead', onMessagesRead);
      connection.off('userOnlineStatusChanged', onUserOnlineStatusChanged);
      connection.off('giftReceived', onGiftReceived);
    };
  }, [connection, isConnected, conversationId]);

  const sendMessage = useCallback(
    async (content: string, type = 'text', metadata?: Record<string, unknown>) => {
      if (!connection || !isConnected) return;
      await connection.invoke('SendPrivateMessage', conversationId, content, type, metadata ?? null);
    },
    [connection, isConnected, conversationId],
  );

  const markRead = useCallback(
    async (messageId: string) => {
      if (!connection || !isConnected) return;
      await connection.invoke('MarkRead', conversationId, messageId);
    },
    [connection, isConnected, conversationId],
  );

  const startTyping = useCallback(() => {
    if (!connection || !isConnected) return;
    const now = Date.now();
    if (now - lastTypingSentRef.current < 3000) return;
    lastTypingSentRef.current = now;
    connection.invoke('StartTyping', conversationId).catch(() => {});

    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    typingTimeoutRef.current = setTimeout(() => {
      connection.invoke('StopTyping', conversationId).catch(() => {});
    }, 3000);
  }, [connection, isConnected, conversationId]);

  const stopTyping = useCallback(() => {
    if (!connection || !isConnected) return;
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    connection.invoke('StopTyping', conversationId).catch(() => {});
    lastTypingSentRef.current = 0;
  }, [connection, isConnected, conversationId]);

  const clearPendingGift = useCallback(() => {
    setPendingGift(null);
  }, []);

  // Merge history + new real-time messages
  const historyMessages = history.data?.pages.flatMap((p) => p.items) ?? [];
  const allMessages = [...historyMessages, ...newMessages];

  return {
    messages: allMessages,
    sendMessage,
    markRead,
    typingUserId,
    otherUserOnline,
    isConnected,
    pendingGift,
    clearPendingGift,
    lastReadMessageId,
    startTyping,
    stopTyping,
    fetchNextPage: history.fetchNextPage,
    hasNextPage: history.hasNextPage,
    isLoadingHistory: history.isLoading,
    historyError: history.error,
  };
}
