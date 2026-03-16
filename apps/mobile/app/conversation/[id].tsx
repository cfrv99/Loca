import { View, Pressable, Text } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useQuery } from '@tanstack/react-query';
import { PrivateChatScreen } from '../../features/chat/screens/private-chat';
import { LoadingSkeleton } from '../../shared/components/loading-skeleton';
import { api } from '../../shared/services/api-client';
import type { ApiResponse, ConversationDto } from '../../shared/types';

export default function ConversationScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  // Fetch conversation details to get the other user's name
  const { data: conversations } = useQuery({
    queryKey: ['conversations'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<ConversationDto[]>>('/conversations');
      if (!res.data.success || !res.data.data) return [];
      return res.data.data;
    },
    staleTime: 1000 * 60 * 5,
  });

  const conversation = conversations?.find((c) => c.conversationId === id);
  const otherUserName = conversation?.otherUser.displayName ?? 'Sohbet';

  if (!id) {
    return <LoadingSkeleton variant="chat" />;
  }

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Back button header */}
      <View className="flex-row items-center px-4 pt-12 pb-2 bg-white dark:bg-gray-800">
        <Pressable
          onPress={() => router.back()}
          className="mr-3 py-2"
          accessibilityLabel="Geri"
          accessibilityRole="button"
        >
          <Text className="text-accent text-lg">{'<'} Geri</Text>
        </Pressable>
      </View>

      <PrivateChatScreen
        conversationId={id}
        otherUserName={otherUserName}
      />
    </View>
  );
}
