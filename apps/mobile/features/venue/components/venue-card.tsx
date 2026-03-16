import { View, Text, Image } from 'react-native';
import type { VenueCardDto } from '../../../shared/types';

interface Props {
  venue: VenueCardDto;
}

export function VenueCard({ venue }: Props) {
  const activityColor =
    venue.activityLevel === 'high'
      ? 'bg-success'
      : venue.activityLevel === 'medium'
        ? 'bg-warning'
        : 'bg-gray-400';

  const formatDistance = (meters: number): string => {
    if (meters < 1000) return `${Math.round(meters)}m`;
    return `${(meters / 1000).toFixed(1)}km`;
  };

  return (
    <View className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm overflow-hidden">
      {venue.coverPhotoUrl ? (
        <Image
          source={{ uri: venue.coverPhotoUrl }}
          className="w-full h-40"
          accessibilityLabel={`${venue.name} foto`}
        />
      ) : (
        <View className="w-full h-40 bg-gray-200 dark:bg-gray-700 items-center justify-center">
          <Text className="text-4xl">🏢</Text>
        </View>
      )}

      <View className="p-4">
        <View className="flex-row items-center justify-between">
          <Text className="text-lg font-semibold text-primary dark:text-white flex-1">
            {venue.name}
          </Text>
          <View className={`w-2.5 h-2.5 rounded-full ${activityColor} ml-2`} />
        </View>

        <Text className="text-sm text-gray-500 mt-1" numberOfLines={1}>
          {venue.address}
        </Text>

        <View className="flex-row items-center mt-2 gap-3">
          <Text className="text-sm font-medium text-accent">
            {venue.stats.total} nəfər
          </Text>
          <Text className="text-xs text-gray-400">
            {venue.stats.male}♂ · {venue.stats.female}♀
          </Text>
          <Text className="text-xs text-gray-400 ml-auto">
            {formatDistance(venue.distanceMeters)}
          </Text>
        </View>

        {venue.activeGames > 0 && (
          <Text className="text-xs text-secondary mt-1">
            🎮 {venue.activeGames} aktiv oyun
          </Text>
        )}

        {venue.chatMessageCount > 0 && (
          <Text className="text-xs text-gray-400 mt-1">
            Chat-da nə baş verir? QR scan et və qoşul!
          </Text>
        )}
      </View>
    </View>
  );
}
