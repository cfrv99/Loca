import { useState } from 'react';
import { View, Text, Pressable, FlatList, Image, RefreshControl } from 'react-native';
import { useRouter } from 'expo-router';
import { useMatchInbox, useConversations, useRespondToMatch } from '../../../features/matching/hooks/use-matches';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { EmptyState } from '../../../shared/components/empty-state';
import type { MatchRequestDto, ConversationDto } from '../../../shared/types';

type MatchTab = 'conversations' | 'pending' | 'accepted' | 'declined';

function ConversationItem({ conversation }: { conversation: ConversationDto }) {
  const router = useRouter();

  const formatTime = (dateStr: string): string => {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    if (minutes < 60) return `${minutes}d`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}s`;
    return `${Math.floor(hours / 24)}g`;
  };

  return (
    <Pressable
      onPress={() => router.push(`/conversation/${conversation.conversationId}`)}
      className="flex-row items-center px-4 py-3 bg-white dark:bg-gray-800 border-b border-gray-100 dark:border-gray-700"
      accessibilityRole="button"
      accessibilityLabel={`${conversation.otherUser.displayName} ilə söhbət`}
    >
      {/* Avatar */}
      <View className="w-12 h-12 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center mr-3">
        {conversation.otherUser.avatarUrl ? (
          <Image
            source={{ uri: conversation.otherUser.avatarUrl }}
            className="w-12 h-12 rounded-full"
            accessibilityLabel={`${conversation.otherUser.displayName} avatarı`}
          />
        ) : (
          <Text className="text-lg">👤</Text>
        )}
      </View>

      {/* Content */}
      <View className="flex-1 mr-2">
        <Text className="text-base font-semibold text-primary dark:text-white">
          {conversation.otherUser.displayName}
        </Text>
        {conversation.lastMessage && (
          <Text
            className="text-sm text-gray-500 dark:text-gray-400 mt-0.5"
            numberOfLines={1}
          >
            {conversation.lastMessage.content}
          </Text>
        )}
      </View>

      {/* Meta */}
      <View className="items-end">
        <Text className="text-xs text-gray-400">
          {formatTime(conversation.updatedAt)}
        </Text>
        {conversation.unreadCount > 0 && (
          <View className="bg-accent rounded-full min-w-[20px] h-5 items-center justify-center mt-1 px-1.5">
            <Text className="text-xs text-white font-bold">
              {conversation.unreadCount}
            </Text>
          </View>
        )}
      </View>
    </Pressable>
  );
}

function MatchRequestItem({
  request,
  onRespond,
}: {
  request: MatchRequestDto;
  onRespond: (matchId: string, action: 'accept' | 'decline') => void;
}) {
  return (
    <View className="flex-row items-center px-4 py-3 bg-white dark:bg-gray-800 border-b border-gray-100 dark:border-gray-700">
      {/* Avatar */}
      <View className="w-12 h-12 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center mr-3">
        {request.senderAvatar ? (
          <Image
            source={{ uri: request.senderAvatar }}
            className="w-12 h-12 rounded-full"
            accessibilityLabel={`${request.senderName} avatarı`}
          />
        ) : (
          <Text className="text-lg">👤</Text>
        )}
      </View>

      {/* Content */}
      <View className="flex-1 mr-2">
        <Text className="text-base font-semibold text-primary dark:text-white">
          {request.senderName}
        </Text>
        {request.introMessage && (
          <Text
            className="text-sm text-gray-500 dark:text-gray-400 mt-0.5"
            numberOfLines={2}
          >
            {request.introMessage}
          </Text>
        )}
      </View>

      {/* Actions (only for pending) */}
      {request.status === 'pending' && (
        <View className="flex-row gap-2">
          <Pressable
            onPress={() => onRespond(request.id, 'decline')}
            className="w-10 h-10 rounded-full border border-error items-center justify-center"
            accessibilityRole="button"
            accessibilityLabel="Rədd et"
          >
            <Text className="text-error text-lg">X</Text>
          </Pressable>
          <Pressable
            onPress={() => onRespond(request.id, 'accept')}
            className="w-10 h-10 rounded-full bg-success items-center justify-center"
            accessibilityRole="button"
            accessibilityLabel="Qəbul et"
          >
            <Text className="text-white text-lg font-bold">{'✓'}</Text>
          </Pressable>
        </View>
      )}

      {/* Status badge for non-pending */}
      {request.status !== 'pending' && (
        <View
          className={`rounded-full px-3 py-1 ${
            request.status === 'accepted'
              ? 'bg-success/20'
              : 'bg-gray-200 dark:bg-gray-700'
          }`}
        >
          <Text
            className={`text-xs font-medium ${
              request.status === 'accepted'
                ? 'text-success'
                : 'text-gray-500'
            }`}
          >
            {request.status === 'accepted' ? 'Qəbul' : 'Rədd'}
          </Text>
        </View>
      )}
    </View>
  );
}

export default function MatchesScreen() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<MatchTab>('conversations');

  const {
    data: conversations,
    isLoading: convLoading,
    error: convError,
    refetch: convRefetch,
    isRefetching: convRefetching,
  } = useConversations();

  const {
    data: pendingRequests,
    isLoading: pendingLoading,
    error: pendingError,
    refetch: pendingRefetch,
    isRefetching: pendingRefetching,
  } = useMatchInbox('pending');

  const respondMutation = useRespondToMatch();

  const handleRespond = (matchId: string, action: 'accept' | 'decline') => {
    respondMutation.mutate({ matchId, action });
  };

  const pendingCount = pendingRequests?.length ?? 0;

  const tabs: { key: MatchTab; label: string; badge?: number }[] = [
    { key: 'conversations', label: 'Söhbətlər' },
    { key: 'pending', label: 'Gözləyən', badge: pendingCount },
  ];

  const renderContent = () => {
    if (activeTab === 'conversations') {
      if (convLoading) return <LoadingSkeleton variant="chat" />;
      if (convError) return <ErrorState message="Söhbətlər yüklənmədi" onRetry={convRefetch} />;
      if (!conversations || conversations.length === 0) {
        return <EmptyState icon="💬" message="Hələ heç bir söhbət yoxdur" />;
      }
      return (
        <FlatList
          data={conversations}
          keyExtractor={(item) => item.conversationId}
          renderItem={({ item }) => <ConversationItem conversation={item} />}
          refreshControl={
            <RefreshControl refreshing={convRefetching} onRefresh={convRefetch} />
          }
          showsVerticalScrollIndicator={false}
          contentContainerStyle={{ paddingBottom: 100 }}
        />
      );
    }

    if (activeTab === 'pending') {
      if (pendingLoading) return <LoadingSkeleton variant="default" />;
      if (pendingError) return <ErrorState message="Sorğular yüklənmədi" onRetry={pendingRefetch} />;
      if (!pendingRequests || pendingRequests.length === 0) {
        return <EmptyState icon="📨" message="Gözləyən sorğu yoxdur" />;
      }
      return (
        <FlatList
          data={pendingRequests}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <MatchRequestItem request={item} onRespond={handleRespond} />
          )}
          refreshControl={
            <RefreshControl refreshing={pendingRefetching} onRefresh={pendingRefetch} />
          }
          showsVerticalScrollIndicator={false}
          contentContainerStyle={{ paddingBottom: 100 }}
        />
      );
    }

    return null;
  };

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="px-4 pt-12 pb-2">
        <Text className="text-3xl font-bold text-primary dark:text-white">
          Matçlar
        </Text>
      </View>

      {/* Tab Bar */}
      <View className="flex-row px-4 border-b border-gray-200 dark:border-gray-700">
        {tabs.map((tab) => (
          <Pressable
            key={tab.key}
            onPress={() => setActiveTab(tab.key)}
            className={`flex-row items-center py-3 mr-6 border-b-2 ${
              activeTab === tab.key
                ? 'border-accent'
                : 'border-transparent'
            }`}
            accessibilityRole="tab"
            accessibilityLabel={tab.label}
            accessibilityState={{ selected: activeTab === tab.key }}
          >
            <Text
              className={`text-sm font-medium ${
                activeTab === tab.key
                  ? 'text-accent'
                  : 'text-gray-500 dark:text-gray-400'
              }`}
            >
              {tab.label}
            </Text>
            {tab.badge != null && tab.badge > 0 && (
              <View className="bg-error rounded-full min-w-[18px] h-[18px] items-center justify-center ml-1.5 px-1">
                <Text className="text-xs text-white font-bold">
                  {tab.badge}
                </Text>
              </View>
            )}
          </Pressable>
        ))}
      </View>

      {/* Content */}
      <View className="flex-1">{renderContent()}</View>
    </View>
  );
}
