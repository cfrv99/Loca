import { View, FlatList, KeyboardAvoidingView, Platform, Text } from 'react-native';
import { useRef, useEffect } from 'react';
import { usePrivateChat } from '../hooks/use-private-chat';
import { ChatBubble } from '../components/chat-bubble';
import { TypingIndicator } from '../components/typing-indicator';
import { MessageInput } from '../components/message-input';
import { ConnectionStatus } from '../components/connection-status';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { useAuthStore } from '../../../shared/stores/auth-store';
import type { PrivateMessageDto, ChatMessageDto } from '../../../shared/types';

interface Props {
  conversationId: string;
  otherUserName: string;
  onGiftPress?: () => void;
}

function formatLastSeen(isoDate?: string): string {
  if (!isoDate) return '';
  const now = new Date();
  const seen = new Date(isoDate);
  const diffMs = now.getTime() - seen.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return 'indicə';
  if (diffMin < 60) return `${diffMin} dəq əvvəl`;
  const diffHours = Math.floor(diffMin / 60);
  if (diffHours < 24) return `${diffHours} saat əvvəl`;
  return `${Math.floor(diffHours / 24)} gun əvvəl`;
}

/** Map a PrivateMessageDto to ChatMessageDto for ChatBubble reuse */
function toChat(msg: PrivateMessageDto): ChatMessageDto {
  return {
    id: msg.id,
    senderId: msg.senderId,
    senderName: msg.senderName,
    senderAvatar: msg.senderAvatar,
    type: msg.type,
    content: msg.content,
    mediaUrl: msg.mediaUrl,
    replyTo: msg.replyTo ? toChat(msg.replyTo) : undefined,
    metadata: msg.metadata,
    createdAt: msg.createdAt,
  };
}

export function PrivateChatScreen({ conversationId, otherUserName, onGiftPress }: Props) {
  const {
    messages,
    sendMessage,
    markRead,
    typingUserId,
    otherUserOnline,
    isConnected,
    lastReadMessageId,
    startTyping,
    stopTyping,
    fetchNextPage,
    hasNextPage,
    isLoadingHistory,
    historyError,
  } = usePrivateChat(conversationId);

  const { user } = useAuthStore();
  const flatListRef = useRef<FlatList<PrivateMessageDto>>(null);

  // Mark latest message as read when visible (with inverted list, first item is newest)
  useEffect(() => {
    if (messages.length > 0) {
      const lastMsg = messages[messages.length - 1];
      if (lastMsg.senderId !== user?.id) {
        markRead(lastMsg.id).catch(() => {});
      }
    }
  }, [messages, markRead, user?.id]);

  if (isLoadingHistory) return <LoadingSkeleton variant="chat" />;
  if (historyError) return <ErrorState message="Mesajlar yuklenmədi" onRetry={() => {}} />;

  const onlineStatusText = otherUserOnline.isOnline
    ? 'Onlayn'
    : otherUserOnline.lastSeenAt
      ? `Son gorulmə: ${formatLastSeen(otherUserOnline.lastSeenAt)}`
      : '';

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background-light dark:bg-background-dark"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={90}
    >
      {/* Connection status */}
      <ConnectionStatus isConnected={isConnected} />

      {/* Header with online status */}
      <View className="flex-row items-center px-4 py-2 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <View className="flex-1">
          <Text className="text-lg font-semibold text-primary dark:text-white">
            {otherUserName}
          </Text>
          {onlineStatusText !== '' && (
            <View className="flex-row items-center">
              {otherUserOnline.isOnline && (
                <View className="w-2 h-2 rounded-full bg-success mr-1" />
              )}
              <Text className="text-xs text-gray-500 dark:text-gray-400">
                {onlineStatusText}
              </Text>
            </View>
          )}
        </View>
      </View>

      {/* Messages — inverted FlatList for chat UX */}
      <FlatList
        ref={flatListRef}
        data={[...messages].reverse()}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => {
          const isOwn = item.senderId === user?.id;
          const chatMsg = toChat(item);
          return (
            <View>
              <ChatBubble message={chatMsg} isOwn={isOwn} />
              {/* Read receipt for own messages */}
              {isOwn && lastReadMessageId && item.id <= lastReadMessageId && (
                <View className="flex-row justify-end pr-4 -mt-1 mb-1">
                  <Text className="text-xs text-accent">✓✓</Text>
                </View>
              )}
            </View>
          );
        }}
        inverted
        onEndReached={() => {
          if (hasNextPage) fetchNextPage();
        }}
        onEndReachedThreshold={0.3}
        contentContainerStyle={{ paddingHorizontal: 16, paddingBottom: 8, paddingTop: 8 }}
        showsVerticalScrollIndicator={false}
      />

      {/* Typing indicator */}
      {typingUserId && <TypingIndicator names={[otherUserName]} />}

      {/* Input bar */}
      <MessageInput
        onSend={(content, type) => sendMessage(content, type)}
        onTyping={startTyping}
        onStopTyping={stopTyping}
        onGiftPress={onGiftPress}
        isConnected={isConnected}
      />
    </KeyboardAvoidingView>
  );
}
