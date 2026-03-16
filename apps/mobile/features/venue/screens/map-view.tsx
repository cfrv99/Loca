import { View, Text, Pressable, Dimensions } from 'react-native';
import { useState, useMemo } from 'react';
import { useVenuesNearby } from '../hooks/use-venues-query';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import type { VenueCardDto } from '../../../shared/types';

interface Props {
  onVenuePress: (venueId: string) => void;
  onToggleList: () => void;
}

// react-native-maps requires native setup; this provides a placeholder-based map view
// that displays venue data in a grid overlay. When react-native-maps is configured,
// replace the placeholder with actual MapView + Marker components.

const { width: SCREEN_WIDTH } = Dimensions.get('window');

function getActivityColor(level: string): string {
  switch (level) {
    case 'high': return 'bg-success';
    case 'medium': return 'bg-warning';
    default: return 'bg-gray-400';
  }
}

function getMarkerSize(total: number): number {
  if (total > 20) return 48;
  if (total > 5) return 36;
  return 28;
}

export function MapView({ onVenuePress, onToggleList }: Props) {
  const { data, isLoading, error, refetch } = useVenuesNearby();
  const [selectedVenue, setSelectedVenue] = useState<VenueCardDto | null>(null);

  const venues = useMemo(() => {
    return data?.pages.flatMap((p) => p.items) ?? [];
  }, [data]);

  if (isLoading) return <LoadingSkeleton variant="default" />;
  if (error) return <ErrorState message="Xəritə yuklenmədi" onRetry={refetch} />;

  const handleVenueSelect = (venue: VenueCardDto) => {
    setSelectedVenue(venue);
  };

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Map area placeholder */}
      <View className="flex-1 bg-gray-100 dark:bg-gray-900 items-center justify-center relative">
        {/* Placeholder map background */}
        <View className="absolute inset-0 bg-gray-200 dark:bg-gray-800" />

        {/* Info text */}
        <Text className="text-sm text-gray-500 dark:text-gray-400 absolute top-4">
          Xəritə goruntusu (react-native-maps lazimdir)
        </Text>

        {/* Venue markers plotted as absolute positioned circles */}
        {venues.map((venue, index) => {
          const markerSize = getMarkerSize(venue.stats.total);
          const activityColor = getActivityColor(venue.activityLevel);
          // Distribute markers in a grid pattern
          const col = index % 3;
          const row = Math.floor(index / 3);
          const x = 40 + col * (SCREEN_WIDTH / 3 - 20);
          const y = 60 + row * 100;

          return (
            <Pressable
              key={venue.id}
              onPress={() => handleVenueSelect(venue)}
              style={{
                position: 'absolute',
                left: x,
                top: y,
                width: markerSize,
                height: markerSize,
              }}
              className={`rounded-full items-center justify-center ${activityColor} shadow-sm`}
              accessibilityLabel={`${venue.name}, ${venue.stats.total} nəfər`}
              accessibilityRole="button"
            >
              <Text className="text-white text-xs font-bold">
                {venue.stats.total}
              </Text>
            </Pressable>
          );
        })}

        {/* User location blue dot */}
        <View
          className="absolute bg-accent w-4 h-4 rounded-full border-2 border-white shadow-lg"
          style={{ bottom: 120, left: SCREEN_WIDTH / 2 - 8 }}
          accessibilityLabel="Sizin mekanınız"
        />
      </View>

      {/* Bottom sheet for selected venue */}
      {selectedVenue && (
        <View className="bg-white dark:bg-gray-800 rounded-t-3xl p-4 shadow-lg">
          <View className="flex-row items-center">
            <View className="flex-1">
              <Text className="text-lg font-semibold text-primary dark:text-white">
                {selectedVenue.name}
              </Text>
              <Text className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
                {selectedVenue.address}
              </Text>
              <View className="flex-row items-center mt-1 gap-3">
                <Text className="text-sm font-medium text-accent">
                  {selectedVenue.stats.total} nəfər
                </Text>
                <Text className="text-xs text-gray-400">
                  {selectedVenue.stats.male}♂ · {selectedVenue.stats.female}♀
                </Text>
              </View>
            </View>
            <Pressable
              onPress={() => onVenuePress(selectedVenue.id)}
              className="bg-accent rounded-xl py-2 px-4"
              accessibilityLabel={`${selectedVenue.name} mekanina bax`}
              accessibilityRole="button"
            >
              <Text className="text-white font-semibold text-sm">Bax</Text>
            </Pressable>
          </View>
        </View>
      )}

      {/* Toggle to list view */}
      <Pressable
        onPress={onToggleList}
        className="absolute top-12 right-4 bg-white dark:bg-gray-800 rounded-xl py-2 px-4 shadow-md"
        accessibilityLabel="Siyahi goruntusu"
        accessibilityRole="button"
      >
        <Text className="text-sm font-medium text-primary dark:text-white">
          Siyahi
        </Text>
      </Pressable>
    </View>
  );
}
