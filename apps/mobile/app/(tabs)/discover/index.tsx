import { View, Text, FlatList, Pressable, RefreshControl } from 'react-native';
import { useRouter } from 'expo-router';
import { useVenuesNearby } from '@/features/venue/hooks/use-venues-query';
import { VenueCard } from '@/features/venue/components/venue-card';
import { LoadingSkeleton } from '@/shared/components/loading-skeleton';
import { ErrorState } from '@/shared/components/error-state';
import { EmptyState } from '@/shared/components/empty-state';

export default function DiscoverScreen() {
  const router = useRouter();
  const {
    data,
    isLoading,
    error,
    refetch,
    isRefetching,
    fetchNextPage,
    hasNextPage,
  } = useVenuesNearby();

  if (isLoading) return <LoadingSkeleton variant="venue-list" />;
  if (error) return <ErrorState message="Mekanlar yuklenm edi" onRetry={refetch} />;
  if (!data?.pages?.[0]?.items?.length) {
    return <EmptyState icon="map-pin" message="Yaxinliqda mekan tapilmadi" />;
  }

  const venues = data.pages.flatMap((p) => p.items);

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <View className="px-4 pt-14 pb-2">
        <Text className="text-3xl font-bold text-primary dark:text-white">
          Kesf et
        </Text>
        <Text className="text-sm text-gray-500 mt-1">
          Yaxinliqdaki aktiv mekanlar
        </Text>
      </View>

      <FlatList
        data={venues}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <Pressable
            onPress={() => router.push(`/venue/${item.id}`)}
            className="mx-4 mb-3 active:opacity-80"
            accessibilityRole="button"
            accessibilityLabel={`${item.name}, ${item.activeCount} nefer`}
          >
            <VenueCard venue={item} />
          </Pressable>
        )}
        refreshControl={
          <RefreshControl refreshing={isRefetching} onRefresh={refetch} />
        }
        onEndReached={() => hasNextPage && fetchNextPage()}
        onEndReachedThreshold={0.5}
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 100 }}
      />
    </View>
  );
}
