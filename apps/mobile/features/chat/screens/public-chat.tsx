import { View, FlatList, KeyboardAvoidingView, Platform, Text } from 'react-native';
import { useRef } from 'react';
import { useVenueChat } from '../hooks/use-venue-chat';
import { ChatBubble } from '../components/chat-bubble';
import { TypingIndicator } from '../components/typing-indicator';
import { MessageInput } from '../components/message-input';
import { ConnectionStatus } from '../components/connection-status';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { useAuthStore } from '../../../shared/stores/auth-store';
import type { ChatMessageDto } from '../../../shared/types';

interface Props {
  venueId: string;
  onGiftPress?: () => void;
}

export function PublicChatScreen({ venueId, onGiftPress }: Props) {
  const {
    messages,
    sendMessage,
    sendReaction,
    typingUsers,
    activeUsers,
    isConnected,
    startTyping,
    stopTyping,
    fetchNextPage,
    hasNextPage,
    isLoadingHistory,
    historyError,
  } = useVenueChat(venueId);

  const { user } = useAuthStore();
  const flatListRef = useRef<FlatList<ChatMessageDto>>(null);
  // With inverted FlatList, new messages auto-appear at top (visual bottom)

  if (isLoadingHistory) return <LoadingSkeleton variant="chat" />;
  if (historyError) return <ErrorState message="Mesajlar yuklenmədi" onRetry={() => {}} />;

  const typingNames = Array.from(typingUsers.values());

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background-light dark:bg-background-dark"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={90}
    >
      {/* Connection status */}
      <ConnectionStatus isConnected={isConnected} />

      {/* Active users count */}
      <View className="flex-row items-center justify-center py-1.5 bg-white/50 dark:bg-gray-800/50">
        <View className="w-2 h-2 rounded-full bg-success mr-1.5" />
        <Text className="text-xs text-gray-500 dark:text-gray-400">
          {activeUsers.total} nəfər burada
        </Text>
      </View>

      {/* Messages — inverted FlatList so new messages appear at bottom,
          scrolling up triggers onEndReached for loading older history */}
      <FlatList
        ref={flatListRef}
        data={[...messages].reverse()}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <ChatBubble
            message={item}
            isOwn={item.senderId === user?.id}
            onReaction={sendReaction}
          />
        )}
        inverted
        onEndReached={() => {
          if (hasNextPage) fetchNextPage();
        }}
        onEndReachedThreshold={0.3}
        contentContainerStyle={{ paddingHorizontal: 16, paddingBottom: 8, paddingTop: 8 }}
        showsVerticalScrollIndicator={false}
      />

      {/* Typing indicator */}
      {typingNames.length > 0 && <TypingIndicator names={typingNames} />}

      {/* Input bar */}
      <MessageInput
        onSend={sendMessage}
        onTyping={startTyping}
        onStopTyping={stopTyping}
        onGiftPress={onGiftPress}
        isConnected={isConnected}
      />
    </KeyboardAvoidingView>
  );
}
