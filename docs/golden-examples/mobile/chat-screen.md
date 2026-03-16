# Golden Example: Real-time Chat Screen (Mobile)

## Chat Hook (data layer)
```tsx
import { useState, useEffect, useCallback } from 'react';
import { useSignalR } from '@/shared/hooks/use-signalr';
import { useInfiniteQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/services/api-client';
import type { ChatMessageDto } from '@/shared/types';

export function useVenueChat(venueId: string) {
  const { connection, isConnected } = useSignalR('/hubs/venue-chat');
  const queryClient = useQueryClient();
  const [newMessages, setNewMessages] = useState<ChatMessageDto[]>([]);
  const [typingUsers, setTypingUsers] = useState<Map<string, string>>(new Map());

  // Load history (cursor pagination, scroll up = older)
  const history = useInfiniteQuery({
    queryKey: ['chat', venueId, 'history'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get(`/venues/${venueId}/messages`, {
        params: { cursor: pageParam, pageSize: 50 },
      });
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => last.hasMore ? last.nextCursor : undefined,
    staleTime: Infinity, // history doesn't change
  });

  // Join venue + listen for real-time messages
  useEffect(() => {
    if (!connection || !isConnected) return;

    connection.invoke('JoinVenue', venueId);

    connection.on('receiveMessage', (msg: ChatMessageDto) => {
      setNewMessages((prev) => [...prev, msg]);
    });

    connection.on('typingStarted', (userId: string, name: string) => {
      setTypingUsers((prev) => new Map(prev).set(userId, name));
    });

    connection.on('typingStopped', (userId: string) => {
      setTypingUsers((prev) => { const m = new Map(prev); m.delete(userId); return m; });
    });

    connection.on('giftReceived', (gift) => {
      // Trigger Lottie animation overlay
    });

    return () => {
      connection.invoke('LeaveVenue', venueId);
      connection.off('receiveMessage');
      connection.off('typingStarted');
      connection.off('typingStopped');
      connection.off('giftReceived');
    };
  }, [connection, isConnected, venueId]);

  const sendMessage = useCallback(async (content: string, type = 'text') => {
    if (!connection || !isConnected) return;
    await connection.invoke('SendMessage', venueId, content, type, null, null);
  }, [connection, isConnected, venueId]);

  // Merge history + new real-time messages
  const allMessages = [
    ...(history.data?.pages.flatMap((p) => p.items) ?? []),
    ...newMessages,
  ];

  return { messages: allMessages, sendMessage, typingUsers, isConnected, ...history };
}
```

## Chat Screen
```tsx
import { View, FlatList, TextInput, Pressable, Text, KeyboardAvoidingView, Platform } from 'react-native';
import { useLocalSearchParams } from 'expo-router';
import { useVenueChat } from '../hooks/use-venue-chat';
import { ChatBubble } from '../components/chat-bubble';
import { TypingIndicator } from '../components/typing-indicator';
import { useAuthStore } from '@/shared/stores/auth-store';

export default function PublicChatScreen() {
  const { venueId } = useLocalSearchParams<{ venueId: string }>();
  const { messages, sendMessage, typingUsers, isConnected, fetchNextPage, hasNextPage } = useVenueChat(venueId);
  const [input, setInput] = useState('');
  const flatListRef = useRef<FlatList>(null);
  const { user } = useAuthStore();

  const handleSend = () => {
    if (!input.trim()) return;
    sendMessage(input.trim());
    setInput('');
  };

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background-light dark:bg-background-dark"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={90}
    >
      {/* Connection status */}
      {!isConnected && (
        <View className="bg-warning px-4 py-1">
          <Text className="text-xs text-center text-white">Bağlantı bərpa edilir...</Text>
        </View>
      )}

      {/* Messages */}
      <FlatList
        ref={flatListRef}
        data={messages}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <ChatBubble message={item} isOwn={item.senderId === user?.id} />
        )}
        inverted={false}
        onEndReached={() => hasNextPage && fetchNextPage()}
        onEndReachedThreshold={0.3}
        contentContainerStyle={{ paddingHorizontal: 16, paddingBottom: 8 }}
      />

      {/* Typing indicator */}
      {typingUsers.size > 0 && (
        <TypingIndicator names={Array.from(typingUsers.values())} />
      )}

      {/* Input bar */}
      <View className="flex-row items-center px-4 py-2 bg-white dark:bg-gray-800 border-t border-gray-200">
        <TextInput
          className="flex-1 bg-gray-100 dark:bg-gray-700 rounded-2xl px-4 py-2 text-base"
          placeholder="Mesaj yaz..."
          value={input}
          onChangeText={setInput}
          multiline
          maxLength={1000}
        />
        <Pressable
          onPress={handleSend}
          disabled={!input.trim() || !isConnected}
          className="ml-2 bg-accent rounded-full w-10 h-10 items-center justify-center"
          accessibilityLabel="Mesaj göndər"
          accessibilityRole="button"
        >
          <Text className="text-white text-lg">→</Text>
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}
```

## Key Rules
- useSignalR hook FIRST, then build UI on top
- FlatList (not ScrollView) for messages
- KeyboardAvoidingView for iOS input
- Connection status bar when disconnected
- Typing indicator component
- inverted={false} + auto-scroll to bottom on new message
- Load more on scroll up (cursor pagination)
