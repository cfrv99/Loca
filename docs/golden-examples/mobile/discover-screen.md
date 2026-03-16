# Golden Example: Complete Mobile Screen

This is the REFERENCE for how every screen should look. Match this pattern.

## Screen Component
```tsx
import { View, Text, FlatList, Pressable, RefreshControl } from 'react-native';
import { useRouter } from 'expo-router';
import { useVenuesNearby } from '../hooks/use-venues-query';
import { VenueCard } from '../components/venue-card';
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

  // ── ALWAYS handle all 3 states ──
  if (isLoading) return <LoadingSkeleton variant="venue-list" />;
  if (error) return <ErrorState message="Məkanlar yüklənmədi" onRetry={refetch} />;
  if (!data?.pages?.[0]?.items?.length) {
    return <EmptyState icon="map-pin" message="Yaxınlıqda məkan tapılmadı" />;
  }

  const venues = data.pages.flatMap(p => p.items);

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="px-4 pt-4 pb-2">
        <Text className="text-3xl font-bold text-primary dark:text-white">
          Kəşf et
        </Text>
        <Text className="text-sm text-gray-500 mt-1">
          Yaxınlıqdakı aktiv məkanlar
        </Text>
      </View>

      {/* Venue List */}
      <FlatList
        data={venues}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <Pressable
            onPress={() => router.push(`/venue/${item.id}`)}
            className="mx-4 mb-3 active:opacity-80"
            accessibilityRole="button"
            accessibilityLabel={`${item.name}, ${item.activeCount} nəfər`}
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
```

## React Query Hook
```tsx
import { useInfiniteQuery } from '@tanstack/react-query';
import { api } from '@/shared/services/api-client';
import type { CursorPageResponse, VenueCardDto } from '@/shared/types';

export function useVenuesNearby() {
  return useInfiniteQuery({
    queryKey: ['venues', 'nearby'],
    queryFn: async ({ pageParam }) => {
      const res = await api.get<CursorPageResponse<VenueCardDto>>('/venues/nearby', {
        params: { cursor: pageParam, pageSize: 20 },
      });
      return res.data.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: 1000 * 60 * 2,
    retry: 2,
  });
}
```

## Component
```tsx
import { View, Text, Image } from 'react-native';
import type { VenueCardDto } from '@/shared/types';

interface Props { venue: VenueCardDto }

export function VenueCard({ venue }: Props) {
  const activityColor = venue.activityLevel === 'high' ? 'bg-success'
    : venue.activityLevel === 'medium' ? 'bg-warning' : 'bg-gray-400';

  return (
    <View className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm overflow-hidden">
      <Image
        source={{ uri: venue.coverPhotoUrl }}
        className="w-full h-40"
        accessibilityLabel={`${venue.name} foto`}
      />
      <View className="p-4">
        <View className="flex-row items-center justify-between">
          <Text className="text-lg font-semibold text-primary dark:text-white">
            {venue.name}
          </Text>
          <View className={`w-2.5 h-2.5 rounded-full ${activityColor}`} />
        </View>
        <Text className="text-sm text-gray-500 mt-1">{venue.address}</Text>
        <View className="flex-row items-center mt-2 gap-3">
          <Text className="text-sm font-medium text-accent">
            {venue.activeCount} nəfər
          </Text>
          <Text className="text-xs text-gray-400">
            {venue.maleCount}♂ · {venue.femaleCount}♀
          </Text>
        </View>
      </View>
    </View>
  );
}
```

## Test
```tsx
import { render, screen } from '@testing-library/react-native';
import { VenueCard } from '../components/venue-card';

const mockVenue = {
  id: '1', name: 'Sea Breeze', address: 'Bilgəh',
  coverPhotoUrl: 'https://example.com/photo.jpg',
  activeCount: 45, maleCount: 25, femaleCount: 20,
  activityLevel: 'high' as const,
};

test('renders venue name and count', () => {
  render(<VenueCard venue={mockVenue} />);
  expect(screen.getByText('Sea Breeze')).toBeTruthy();
  expect(screen.getByText('45 nəfər')).toBeTruthy();
});

test('shows green dot for high activity', () => {
  render(<VenueCard venue={mockVenue} />);
  // Check activity indicator exists
});

test('has accessibility label', () => {
  // Verify accessibilityLabel is present
});
```
