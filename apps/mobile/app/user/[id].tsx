import { useState } from 'react';
import { View, Text, ScrollView, Pressable, Image, Alert } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { usePublicProfile } from '../../features/profile/hooks/use-public-profile';
import { useVenueStore } from '../../shared/stores/venue-store';
import { useAuthStore } from '../../shared/stores/auth-store';
import { api } from '../../shared/services/api-client';
import { LoadingSkeleton } from '../../shared/components/loading-skeleton';
import { ErrorState } from '../../shared/components/error-state';
import { ReportSheet } from '../../features/profile/components/report-sheet';
import { MatchRequestSheet } from '../../features/matching/components/match-request-sheet';
import type { ApiResponse } from '../../shared/types';

export default function UserProfileScreen() {
  const { id: userId } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const { data: profile, isLoading, error, refetch } = usePublicProfile(userId);
  const { isCheckedIn, currentVenueId } = useVenueStore();
  const { user: currentUser } = useAuthStore();

  const [showReport, setShowReport] = useState(false);
  const [showMatchRequest, setShowMatchRequest] = useState(false);
  const [isBlocking, setIsBlocking] = useState(false);

  if (isLoading) return <LoadingSkeleton variant="profile" />;
  if (error || !profile) {
    return <ErrorState message="Profil yuklenmedi" onRetry={refetch} />;
  }

  const canMatch =
    isCheckedIn &&
    currentVenueId &&
    currentUser?.id !== userId;

  const handleBlock = () => {
    Alert.alert(
      'Blokla',
      'Bu istifadecini bloklamaq isteyirsiniz?',
      [
        { text: 'Legv et', style: 'cancel' },
        {
          text: 'Blokla',
          style: 'destructive',
          onPress: async () => {
            setIsBlocking(true);
            try {
              await api.post(`/users/${userId}/block`);
              Alert.alert('Bloklandı', 'Istifadeci bloklandı');
              router.back();
            } catch {
              Alert.alert('Xəta', 'Bloklama uğursuz oldu');
            } finally {
              setIsBlocking(false);
            }
          },
        },
      ]
    );
  };

  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    return `${date.getDate().toString().padStart(2, '0')}.${(date.getMonth() + 1)
      .toString()
      .padStart(2, '0')}.${date.getFullYear()}`;
  };

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="flex-row items-center justify-between px-4 pt-12 pb-4">
        <Pressable
          onPress={() => router.back()}
          className="py-2 pr-4"
          accessibilityRole="button"
          accessibilityLabel="Geri"
        >
          <Text className="text-accent text-base font-medium">Geri</Text>
        </Pressable>
        <Pressable
          onPress={() => setShowReport(true)}
          className="py-2 pl-4"
          accessibilityRole="button"
          accessibilityLabel="Daha cox secim"
        >
          <Text className="text-gray-500 text-lg">...</Text>
        </Pressable>
      </View>

      <ScrollView
        className="flex-1"
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 120 }}
      >
        {/* Avatar */}
        <View className="items-center mb-4">
          <View className="w-24 h-24 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center">
            {profile.avatarUrl ? (
              <Image
                source={{ uri: profile.avatarUrl }}
                className="w-24 h-24 rounded-full"
                accessibilityLabel={`${profile.displayName} avatarı`}
              />
            ) : (
              <Text className="text-4xl">👤</Text>
            )}
          </View>
          {profile.giftBadge && (
            <View className="bg-secondary/20 rounded-full px-3 py-1 mt-2">
              <Text className="text-xs text-secondary font-medium">
                {profile.giftBadge}
              </Text>
            </View>
          )}
        </View>

        {/* Name, age, gender */}
        <View className="items-center mb-6">
          <Text className="text-2xl font-bold text-primary dark:text-white">
            {profile.displayName}, {profile.age}
          </Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {profile.gender === 'male'
              ? 'Kişi'
              : profile.gender === 'female'
                ? 'Qadın'
                : 'Digər'}
          </Text>
        </View>

        {/* Interests */}
        {profile.interests.length > 0 && (
          <View className="px-6 mb-6">
            <Text className="text-sm font-medium text-primary dark:text-white mb-2">
              Maraqlar
            </Text>
            <View className="flex-row flex-wrap gap-2">
              {profile.interests.map((interest) => (
                <View
                  key={interest}
                  className="bg-accent/10 rounded-full px-3 py-1"
                >
                  <Text className="text-sm text-accent">{interest}</Text>
                </View>
              ))}
            </View>
          </View>
        )}

        {/* Purposes */}
        {profile.purposes.length > 0 && (
          <View className="px-6 mb-6">
            <Text className="text-sm font-medium text-primary dark:text-white mb-2">
              Məqsəd
            </Text>
            <View className="flex-row flex-wrap gap-2">
              {profile.purposes.map((purpose) => (
                <View
                  key={purpose}
                  className="bg-secondary/10 rounded-full px-3 py-1"
                >
                  <Text className="text-sm text-secondary">{purpose}</Text>
                </View>
              ))}
            </View>
          </View>
        )}

        {/* Past Venues */}
        {profile.pastVenues.length > 0 && (
          <View className="px-6 mb-6">
            <Text className="text-sm font-medium text-primary dark:text-white mb-2">
              Keçmiş məkanlar
            </Text>
            {profile.pastVenues.map((pv) => (
              <View
                key={`${pv.venueId}-${pv.lastVisit}`}
                className="flex-row items-center justify-between bg-white dark:bg-gray-800 rounded-xl p-3 mb-2"
              >
                <Text className="text-sm text-primary dark:text-white">
                  {pv.venueName}
                </Text>
                <Text className="text-xs text-gray-400">
                  {formatDate(pv.lastVisit)}
                </Text>
              </View>
            ))}
          </View>
        )}

        {/* Stats */}
        <View className="flex-row justify-around px-6 mb-6">
          <View className="items-center">
            <Text className="text-xl font-bold text-accent">
              {profile.memoriesCount}
            </Text>
            <Text className="text-xs text-gray-500">Xatirə</Text>
          </View>
          <View className="items-center">
            <Text className="text-xl font-bold text-accent">
              {profile.pastVenues.length}
            </Text>
            <Text className="text-xs text-gray-500">Məkan</Text>
          </View>
        </View>

        {/* Block button */}
        <View className="px-6">
          <Pressable
            onPress={handleBlock}
            disabled={isBlocking}
            className="border border-error rounded-xl py-3 items-center"
            accessibilityRole="button"
            accessibilityLabel="Bu istifadecini blokla"
          >
            <Text className="text-error font-medium text-sm">
              {isBlocking ? 'Yüklənir...' : 'Blokla'}
            </Text>
          </Pressable>
        </View>
      </ScrollView>

      {/* Match Request CTA */}
      {canMatch && (
        <View className="absolute bottom-0 left-0 right-0 px-4 pb-8 pt-4 bg-background-light/95 dark:bg-background-dark/95">
          <Pressable
            onPress={() => setShowMatchRequest(true)}
            className="bg-accent rounded-2xl py-4 items-center"
            accessibilityRole="button"
            accessibilityLabel="Tanışlıq sorğusu göndər"
          >
            <Text className="text-white font-bold text-base">
              Tanışlıq sorğusu
            </Text>
          </Pressable>
        </View>
      )}

      {/* Report Sheet */}
      {showReport && (
        <ReportSheet
          userId={userId}
          onClose={() => setShowReport(false)}
          onBlock={handleBlock}
        />
      )}

      {/* Match Request Sheet */}
      {showMatchRequest && currentVenueId && (
        <MatchRequestSheet
          receiverId={userId}
          venueId={currentVenueId}
          onClose={() => setShowMatchRequest(false)}
          onSuccess={() => {
            setShowMatchRequest(false);
            Alert.alert('Göndərildi', 'Tanışlıq sorğusu göndərildi!');
          }}
        />
      )}
    </View>
  );
}
