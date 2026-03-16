import { View, Text, FlatList, Pressable, Image, RefreshControl } from 'react-native';
import { useVenueFeed, useToggleLike } from '../hooks/use-venue-feed';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { EmptyState } from '../../../shared/components/empty-state';
import type { PostDto } from '../../../shared/types';

interface Props {
  venueId: string;
}

function PostCard({
  post,
  onLike,
}: {
  post: PostDto;
  onLike: (postId: string) => void;
}) {
  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);

    if (minutes < 1) return 'indi';
    if (minutes < 60) return `${minutes} dəq əvvəl`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} saat əvvəl`;
    const days = Math.floor(hours / 24);
    return `${days} gün əvvəl`;
  };

  return (
    <View className="bg-white dark:bg-gray-800 rounded-2xl mb-3 mx-4 overflow-hidden">
      {/* Author */}
      <View className="flex-row items-center p-3">
        <View className="w-10 h-10 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center mr-3">
          {post.userAvatar ? (
            <Image
              source={{ uri: post.userAvatar }}
              className="w-10 h-10 rounded-full"
              accessibilityLabel={`${post.userName} avatarı`}
            />
          ) : (
            <Text className="text-sm">👤</Text>
          )}
        </View>
        <View className="flex-1">
          <Text className="text-sm font-semibold text-primary dark:text-white">
            {post.userName}
          </Text>
          <Text className="text-xs text-gray-400">
            {formatDate(post.createdAt)}
          </Text>
        </View>
      </View>

      {/* Media */}
      {post.mediaUrls.length > 0 && (
        <Image
          source={{ uri: post.mediaUrls[0] }}
          className="w-full h-64"
          accessibilityLabel="Paylasilmis foto"
        />
      )}

      {/* Content */}
      {post.content && (
        <Text className="text-sm text-primary dark:text-white px-3 py-2">
          {post.content}
        </Text>
      )}

      {/* Actions */}
      <View className="flex-row items-center px-3 pb-3 gap-4">
        <Pressable
          onPress={() => onLike(post.id)}
          className="flex-row items-center py-2"
          accessibilityRole="button"
          accessibilityLabel={`Beyən ${post.likeCount}`}
        >
          <Text className="text-base mr-1">
            {post.isLikedByMe ? '❤️' : '🤍'}
          </Text>
          <Text className="text-sm text-gray-500">{post.likeCount}</Text>
        </Pressable>
        <View className="flex-row items-center">
          <Text className="text-base mr-1">💬</Text>
          <Text className="text-sm text-gray-500">{post.commentCount}</Text>
        </View>
      </View>
    </View>
  );
}

export function VenueFeed({ venueId }: Props) {
  const {
    data,
    isLoading,
    error,
    refetch,
    isRefetching,
    fetchNextPage,
    hasNextPage,
  } = useVenueFeed(venueId);

  const likeMutation = useToggleLike(venueId);

  if (isLoading) return <LoadingSkeleton variant="default" />;
  if (error) return <ErrorState message="Lent yuklenmedi" onRetry={refetch} />;

  const posts = data?.pages.flatMap((p) => p.items) ?? [];

  if (posts.length === 0) {
    return (
      <EmptyState icon="📸" message="Hələ heç bir xatirə paylaşılmayıb" />
    );
  }

  return (
    <FlatList
      data={posts}
      keyExtractor={(item) => item.id}
      renderItem={({ item }) => (
        <PostCard post={item} onLike={(id) => likeMutation.mutate(id)} />
      )}
      refreshControl={
        <RefreshControl refreshing={isRefetching} onRefresh={refetch} />
      }
      onEndReached={() => hasNextPage && fetchNextPage()}
      onEndReachedThreshold={0.5}
      showsVerticalScrollIndicator={false}
      contentContainerStyle={{ paddingTop: 8, paddingBottom: 100 }}
    />
  );
}
