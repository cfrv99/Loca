import { View, Text, FlatList, Pressable, Image, RefreshControl } from 'react-native';
import { useRouter } from 'expo-router';
import { useVenuePeople } from '../hooks/use-venue-people';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { EmptyState } from '../../../shared/components/empty-state';
import type { ActiveUserDto } from '../../../shared/types';

interface Props {
  venueId: string;
}

function PersonCard({ person }: { person: ActiveUserDto }) {
  const router = useRouter();

  return (
    <Pressable
      onPress={() => router.push(`/user/${person.userId}`)}
      className="flex-1 bg-white dark:bg-gray-800 rounded-2xl p-3 m-1.5 items-center"
      accessibilityRole="button"
      accessibilityLabel={`${person.displayName}, ${person.age} yaş`}
    >
      {/* Avatar */}
      <View className="w-16 h-16 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center mb-2">
        {person.avatarUrl ? (
          <Image
            source={{ uri: person.avatarUrl }}
            className="w-16 h-16 rounded-full"
            accessibilityLabel={`${person.displayName} avatarı`}
          />
        ) : (
          <Text className="text-2xl">👤</Text>
        )}
      </View>

      {/* Name + Age */}
      <Text
        className="text-sm font-semibold text-primary dark:text-white text-center"
        numberOfLines={1}
      >
        {person.displayName}
      </Text>
      <Text className="text-xs text-gray-500 dark:text-gray-400">
        {person.age} yaş
      </Text>

      {/* Interest Badges (max 3) */}
      <View className="flex-row flex-wrap justify-center mt-2 gap-1">
        {person.interests.slice(0, 3).map((interest) => (
          <View
            key={interest}
            className="bg-accent/10 rounded-full px-2 py-0.5"
          >
            <Text className="text-xs text-accent">{interest}</Text>
          </View>
        ))}
      </View>
    </Pressable>
  );
}

export function PeopleGrid({ venueId }: Props) {
  const {
    data,
    isLoading,
    error,
    refetch,
    isRefetching,
    fetchNextPage,
    hasNextPage,
  } = useVenuePeople(venueId);

  if (isLoading) return <LoadingSkeleton variant="default" />;
  if (error) return <ErrorState message="Insanlar yuklenmedi" onRetry={refetch} />;

  const people = data?.pages.flatMap((p) => p.items).filter((p) => !p.isAnonymous) ?? [];

  if (people.length === 0) {
    return <EmptyState icon="👥" message="Burada hələ heç kim yoxdur" />;
  }

  return (
    <FlatList
      data={people}
      keyExtractor={(item) => item.userId}
      renderItem={({ item }) => <PersonCard person={item} />}
      numColumns={2}
      refreshControl={
        <RefreshControl refreshing={isRefetching} onRefresh={refetch} />
      }
      onEndReached={() => hasNextPage && fetchNextPage()}
      onEndReachedThreshold={0.5}
      showsVerticalScrollIndicator={false}
      contentContainerStyle={{ padding: 8, paddingBottom: 100 }}
    />
  );
}
