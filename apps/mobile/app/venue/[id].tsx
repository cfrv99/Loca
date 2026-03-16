import { View, Text, ScrollView, Pressable, Image, Linking } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useVenueDetail } from '../../features/venue/hooks/use-venue-detail';
import { LoadingSkeleton } from '../../shared/components/loading-skeleton';
import { ErrorState } from '../../shared/components/error-state';

export default function VenueDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const { data: venue, isLoading, error, refetch } = useVenueDetail(id);

  if (isLoading) return <LoadingSkeleton variant="default" />;
  if (error || !venue) {
    return <ErrorState message="Məkan yüklənmədi" onRetry={refetch} />;
  }

  const activityColor =
    venue.stats.total > 20
      ? 'bg-success'
      : venue.stats.total > 5
        ? 'bg-warning'
        : 'bg-gray-400';

  const activityLabel =
    venue.stats.total > 20
      ? 'Aktiv'
      : venue.stats.total > 5
        ? 'Orta'
        : 'Sakit';

  const allPhotos = [
    venue.coverPhotoUrl,
    ...(venue.photoUrls ?? []),
  ].filter(Boolean) as string[];

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <ScrollView
        className="flex-1"
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 120 }}
      >
        {/* Cover Photo Gallery */}
        {allPhotos.length > 0 ? (
          <ScrollView
            horizontal
            pagingEnabled
            showsHorizontalScrollIndicator={false}
            className="h-64"
          >
            {allPhotos.map((uri, idx) => (
              <Image
                key={idx}
                source={{ uri }}
                className="w-screen h-64"
                accessibilityLabel={`${venue.name} foto ${idx + 1}`}
              />
            ))}
          </ScrollView>
        ) : (
          <View className="w-full h-64 bg-gray-200 dark:bg-gray-700 items-center justify-center">
            <Text className="text-5xl">🏢</Text>
          </View>
        )}

        {/* Back Button Overlay */}
        <Pressable
          onPress={() => router.back()}
          className="absolute top-12 left-4 w-10 h-10 rounded-full bg-black/40 items-center justify-center"
          accessibilityRole="button"
          accessibilityLabel="Geri"
        >
          <Text className="text-white text-lg font-bold">{'<'}</Text>
        </Pressable>

        {/* Photo indicator dots */}
        {allPhotos.length > 1 && (
          <View className="flex-row justify-center gap-1 -mt-6 mb-2">
            {allPhotos.map((_, idx) => (
              <View
                key={idx}
                className="w-2 h-2 rounded-full bg-white/70"
              />
            ))}
          </View>
        )}

        {/* Venue Info */}
        <View className="px-4 pt-4">
          {/* Name and Activity */}
          <View className="flex-row items-center justify-between mb-1">
            <Text className="text-2xl font-bold text-primary dark:text-white flex-1">
              {venue.name}
            </Text>
            <View className="flex-row items-center gap-2">
              <View className={`w-3 h-3 rounded-full ${activityColor}`} />
              <Text className="text-xs text-gray-500">{activityLabel}</Text>
            </View>
          </View>

          {/* Category Badge */}
          <View className="flex-row items-center mb-3">
            <View className="bg-accent/10 rounded-full px-3 py-1 mr-2">
              <Text className="text-xs font-medium text-accent">
                {venue.category}
              </Text>
            </View>
            {venue.googleRating && (
              <Text className="text-sm text-warning font-medium">
                {'*'} {venue.googleRating}
              </Text>
            )}
          </View>

          {/* Address */}
          <Text className="text-sm text-gray-500 dark:text-gray-400 mb-4">
            {venue.address}
          </Text>

          {/* Real-time Stats */}
          <View className="bg-white dark:bg-gray-800 rounded-2xl p-4 mb-4">
            <Text className="text-sm font-medium text-primary dark:text-white mb-3">
              Hal-hazırda
            </Text>
            <View className="flex-row justify-around">
              <View className="items-center">
                <Text className="text-2xl font-bold text-accent">
                  {venue.stats.total}
                </Text>
                <Text className="text-xs text-gray-500">Cəmi</Text>
              </View>
              <View className="items-center">
                <Text className="text-2xl font-bold text-accent">
                  {venue.stats.male}
                </Text>
                <Text className="text-xs text-gray-500">Kişi</Text>
              </View>
              <View className="items-center">
                <Text className="text-2xl font-bold text-accent">
                  {venue.stats.female}
                </Text>
                <Text className="text-xs text-gray-500">Qadın</Text>
              </View>
            </View>
          </View>

          {/* Description */}
          {venue.description && (
            <View className="mb-4">
              <Text className="text-sm font-medium text-primary dark:text-white mb-1">
                Haqqında
              </Text>
              <Text className="text-sm text-gray-600 dark:text-gray-300">
                {venue.description}
              </Text>
            </View>
          )}

          {/* Working Hours */}
          {venue.workingHours && (
            <View className="bg-white dark:bg-gray-800 rounded-2xl p-4 mb-4">
              <Text className="text-sm font-medium text-primary dark:text-white mb-1">
                Iş saatları
              </Text>
              <Text className="text-sm text-gray-600 dark:text-gray-300">
                {venue.workingHours}
              </Text>
            </View>
          )}

          {/* Contact */}
          <View className="bg-white dark:bg-gray-800 rounded-2xl p-4 mb-4">
            <Text className="text-sm font-medium text-primary dark:text-white mb-3">
              Əlaqə
            </Text>

            {venue.phone && (
              <Pressable
                onPress={() => Linking.openURL(`tel:${venue.phone}`)}
                className="flex-row items-center py-2"
                accessibilityRole="button"
                accessibilityLabel={`Zəng et ${venue.phone}`}
              >
                <Text className="text-base mr-2">📞</Text>
                <Text className="text-sm text-accent">{venue.phone}</Text>
              </Pressable>
            )}

            {venue.website && (
              <Pressable
                onPress={() => Linking.openURL(venue.website!)}
                className="flex-row items-center py-2"
                accessibilityRole="link"
                accessibilityLabel="Sayta keç"
              >
                <Text className="text-base mr-2">🌐</Text>
                <Text className="text-sm text-accent" numberOfLines={1}>
                  {venue.website}
                </Text>
              </Pressable>
            )}
          </View>

          {/* Map Preview Placeholder */}
          <View className="bg-white dark:bg-gray-800 rounded-2xl h-40 items-center justify-center mb-4">
            <Text className="text-3xl mb-2">🗺️</Text>
            <Text className="text-sm text-gray-500">
              {venue.latitude.toFixed(4)}, {venue.longitude.toFixed(4)}
            </Text>
          </View>
        </View>
      </ScrollView>

      {/* QR Scan CTA - Fixed Bottom */}
      <View className="absolute bottom-0 left-0 right-0 px-4 pb-8 pt-4 bg-background-light/95 dark:bg-background-dark/95">
        <Pressable
          onPress={() => router.push(`/venue/${id}/scan`)}
          className="bg-accent rounded-2xl py-4 items-center shadow-lg"
          accessibilityRole="button"
          accessibilityLabel="QR Scan et və qoşul"
        >
          <Text className="text-white font-bold text-lg">
            QR Scan et və qoşul!
          </Text>
          <Text className="text-white/70 text-xs mt-1">
            Kameranı QR koda yönəldin
          </Text>
        </Pressable>
      </View>
    </View>
  );
}
