import { View, Text, Pressable, FlatList, Image, TextInput, Modal } from 'react-native';
import { useState } from 'react';
import { useVenueFeed } from '../hooks/use-venue-feed';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { EmptyState } from '../../../shared/components/empty-state';
import type { PostDto, CommentDto } from '../../../shared/types';

interface Props {
  venueId: string;
}

function formatTimeAgo(isoDate: string): string {
  const now = new Date();
  const date = new Date(isoDate);
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return 'indicə';
  if (diffMin < 60) return `${diffMin} dəq`;
  const diffHours = Math.floor(diffMin / 60);
  if (diffHours < 24) return `${diffHours} saat`;
  return `${Math.floor(diffHours / 24)} gun`;
}

export function FeedTab({ venueId }: Props) {
  const { data, isLoading, error, refetch, isRefetching, fetchNextPage, hasNextPage, toggleLike, addComment } =
    useVenueFeed(venueId);
  const [commentModalPost, setCommentModalPost] = useState<PostDto | null>(null);
  const [commentText, setCommentText] = useState('');
  const [comments, setComments] = useState<CommentDto[]>([]);

  if (isLoading) return <LoadingSkeleton variant="venue-list" />;
  if (error) return <ErrorState message="Feed yuklenmədi" onRetry={refetch} />;

  const posts = data?.pages.flatMap((p) => p.items) ?? [];
  if (posts.length === 0) {
    return <EmptyState icon="📸" message="Hələ heç bir xatirə yoxdur. İlk postu sən paylas!" />;
  }

  const handleLike = (postId: string) => {
    toggleLike.mutate(postId);
  };

  const handleOpenComments = async (post: PostDto) => {
    setCommentModalPost(post);
    setComments([]);
    // Comments would be loaded here in a full implementation
  };

  const handleSendComment = () => {
    if (!commentText.trim() || !commentModalPost) return;
    addComment.mutate(
      { postId: commentModalPost.id, content: commentText.trim() },
      {
        onSuccess: (newComment) => {
          setComments((prev) => [newComment, ...prev]);
          setCommentText('');
        },
      },
    );
  };

  const renderPost = ({ item }: { item: PostDto }) => (
    <View className="bg-white dark:bg-gray-800 mb-3 rounded-2xl overflow-hidden shadow-sm mx-4">
      {/* Author row */}
      <View className="flex-row items-center px-4 pt-3 pb-2">
        <View className="w-10 h-10 rounded-full bg-gray-200 dark:bg-gray-600 items-center justify-center mr-3">
          {item.userAvatar ? (
            <Image
              source={{ uri: item.userAvatar }}
              className="w-10 h-10 rounded-full"
              accessibilityLabel={`${item.userName} avatar`}
            />
          ) : (
            <Text className="text-base">{item.userName.charAt(0).toUpperCase()}</Text>
          )}
        </View>
        <View className="flex-1">
          <Text className="text-sm font-semibold text-primary dark:text-white">
            {item.userName}
          </Text>
          <Text className="text-xs text-gray-400">{formatTimeAgo(item.createdAt)}</Text>
        </View>
      </View>

      {/* Media */}
      {item.mediaUrls.length > 0 && (
        <Image
          source={{ uri: item.mediaUrls[0] }}
          className="w-full h-64"
          resizeMode="cover"
          accessibilityLabel="Post media"
        />
      )}

      {/* Content */}
      {item.content && (
        <Text className="px-4 py-2 text-base text-primary dark:text-white">
          {item.content}
        </Text>
      )}

      {/* Actions */}
      <View className="flex-row items-center px-4 py-2 gap-4">
        <Pressable
          onPress={() => handleLike(item.id)}
          className="flex-row items-center"
          accessibilityLabel={`Beyən, ${item.likeCount}`}
          accessibilityRole="button"
        >
          <Text className="text-lg mr-1">{item.isLikedByMe ? '❤️' : '🤍'}</Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400">{item.likeCount}</Text>
        </Pressable>

        <Pressable
          onPress={() => handleOpenComments(item)}
          className="flex-row items-center"
          accessibilityLabel={`Sərhlər, ${item.commentCount}`}
          accessibilityRole="button"
        >
          <Text className="text-lg mr-1">💬</Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400">{item.commentCount}</Text>
        </Pressable>
      </View>
    </View>
  );

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Section header */}
      <View className="px-4 py-3">
        <Text className="text-lg font-semibold text-primary dark:text-white">
          Xatirələr
        </Text>
      </View>

      <FlatList
        data={posts}
        keyExtractor={(item) => item.id}
        renderItem={renderPost}
        onEndReached={() => hasNextPage && fetchNextPage()}
        onEndReachedThreshold={0.5}
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 80 }}
      />

      {/* Comment modal */}
      <Modal
        visible={commentModalPost !== null}
        animationType="slide"
        transparent
        onRequestClose={() => setCommentModalPost(null)}
      >
        <View className="flex-1 bg-black/50 justify-end">
          <View className="bg-white dark:bg-gray-800 rounded-t-3xl max-h-[60%] p-4">
            <View className="flex-row items-center justify-between mb-3">
              <Text className="text-lg font-semibold text-primary dark:text-white">
                Sərhlər
              </Text>
              <Pressable
                onPress={() => setCommentModalPost(null)}
                accessibilityLabel="Baglat"
                accessibilityRole="button"
              >
                <Text className="text-gray-500 text-lg">✕</Text>
              </Pressable>
            </View>

            <FlatList
              data={comments}
              keyExtractor={(item) => item.id}
              renderItem={({ item: comment }) => (
                <View className="flex-row mb-3">
                  <View className="w-8 h-8 rounded-full bg-gray-200 dark:bg-gray-600 items-center justify-center mr-2">
                    <Text className="text-xs">{comment.userName.charAt(0)}</Text>
                  </View>
                  <View className="flex-1">
                    <Text className="text-sm font-semibold text-primary dark:text-white">
                      {comment.userName}
                    </Text>
                    <Text className="text-sm text-gray-600 dark:text-gray-300">
                      {comment.content}
                    </Text>
                  </View>
                </View>
              )}
              ListEmptyComponent={
                <Text className="text-sm text-gray-400 text-center py-4">
                  Hələ sərh yoxdur
                </Text>
              }
            />

            {/* Comment input */}
            <View className="flex-row items-center pt-3 border-t border-gray-200 dark:border-gray-700">
              <TextInput
                className="flex-1 bg-gray-100 dark:bg-gray-700 rounded-2xl px-4 py-2 text-base text-primary dark:text-white"
                placeholder="Sərh yaz..."
                placeholderTextColor="#9CA3AF"
                value={commentText}
                onChangeText={setCommentText}
                maxLength={500}
                accessibilityLabel="Sərh yaz"
              />
              <Pressable
                onPress={handleSendComment}
                disabled={!commentText.trim()}
                className="ml-2 bg-accent rounded-full w-10 h-10 items-center justify-center"
                accessibilityLabel="Sərh gondər"
                accessibilityRole="button"
              >
                <Text className="text-white text-lg font-bold">{'>'}</Text>
              </Pressable>
            </View>
          </View>
        </View>
      </Modal>
    </View>
  );
}
