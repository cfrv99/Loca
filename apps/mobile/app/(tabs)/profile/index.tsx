import { View, Text, Pressable, ScrollView, Image } from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '../../../shared/stores/auth-store';

export default function ProfileScreen() {
  const router = useRouter();
  const { user, logout } = useAuthStore();

  const vibeMap = (user?.vibePreferences ?? []).reduce(
    (acc, v) => {
      acc[v.vibe] = Math.round(v.weight * 100);
      return acc;
    },
    {} as Record<string, number>
  );

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 40 }}
      >
        {/* Header */}
        <View className="flex-row items-center justify-between px-4 pt-12 pb-4">
          <Text className="text-3xl font-bold text-primary dark:text-white">
            Profil
          </Text>
          <Pressable
            onPress={() => router.push('/(tabs)/profile/edit')}
            className="py-2 pl-4"
            accessibilityRole="button"
            accessibilityLabel="Profili redaktə et"
          >
            <Text className="text-accent text-base font-medium">
              Redaktə et
            </Text>
          </Pressable>
        </View>

        {/* Avatar */}
        <View className="items-center mb-4">
          <View className="w-24 h-24 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center">
            {user?.avatarUrl ? (
              <Image
                source={{ uri: user.avatarUrl }}
                className="w-24 h-24 rounded-full"
                accessibilityLabel="Profil foto"
              />
            ) : (
              <Text className="text-4xl">👤</Text>
            )}
          </View>
        </View>

        {/* Name + email */}
        <View className="items-center mb-6">
          <Text className="text-xl font-semibold text-primary dark:text-white mb-1">
            {user?.displayName ?? 'Istifadəçi'}
          </Text>
          <Text className="text-sm text-gray-500">
            {user?.email ?? ''}
          </Text>
          {user?.isPremium && (
            <View className="bg-secondary/20 rounded-full px-3 py-1 mt-2">
              <Text className="text-xs text-secondary font-semibold">
                Loca Gold
              </Text>
            </View>
          )}
        </View>

        {/* Stats */}
        <View className="flex-row justify-around px-6 mb-6">
          <View className="items-center">
            <Text className="text-xl font-bold text-accent">
              {user?.coinBalance ?? 0}
            </Text>
            <Text className="text-xs text-gray-500">Coin</Text>
          </View>
          <View className="items-center">
            <Text className="text-xl font-bold text-accent">
              {user?.interests?.length ?? 0}
            </Text>
            <Text className="text-xs text-gray-500">Maraqlar</Text>
          </View>
        </View>

        {/* Interests */}
        {user?.interests && user.interests.length > 0 && (
          <View className="px-6 mb-4">
            <Text className="text-sm font-medium text-primary dark:text-white mb-2">
              Maraqlar
            </Text>
            <View className="flex-row flex-wrap gap-2">
              {user.interests.map((interest) => (
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
        {user?.purposes && user.purposes.length > 0 && (
          <View className="px-6 mb-4">
            <Text className="text-sm font-medium text-primary dark:text-white mb-2">
              Məqsəd
            </Text>
            <View className="flex-row flex-wrap gap-2">
              {user.purposes.map((purpose) => (
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

        {/* Vibe Preferences */}
        {Object.keys(vibeMap).length > 0 && (
          <View className="px-6 mb-6">
            <Text className="text-sm font-medium text-primary dark:text-white mb-2">
              Vibe
            </Text>
            {Object.entries(vibeMap).map(([vibe, weight]) => (
              <View key={vibe} className="mb-2">
                <View className="flex-row items-center justify-between mb-1">
                  <Text className="text-xs text-gray-500 dark:text-gray-400">
                    {vibe === 'Romantic'
                      ? 'Romantik'
                      : vibe === 'Party'
                        ? 'Party'
                        : vibe === 'Chill'
                          ? 'Sakit'
                          : vibe === 'Adventurous'
                            ? 'Macəraperest'
                            : vibe}
                  </Text>
                  <Text className="text-xs text-gray-500">{weight}%</Text>
                </View>
                <View className="h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                  <View
                    className="h-2 bg-accent rounded-full"
                    style={{ width: `${weight}%` }}
                  />
                </View>
              </View>
            ))}
          </View>
        )}

        {/* Menu Items */}
        <View className="px-6 mb-6">
          <Pressable
            onPress={() => router.push('/coin-shop')}
            className="flex-row items-center justify-between bg-white dark:bg-gray-800 rounded-xl p-4 mb-2"
            accessibilityRole="button"
            accessibilityLabel="Coin mağazası"
          >
            <View className="flex-row items-center">
              <Text className="text-base mr-3">🪙</Text>
              <Text className="text-base text-primary dark:text-white">
                Coin mağazası
              </Text>
            </View>
            <Text className="text-gray-400">{'>'}</Text>
          </Pressable>

          <Pressable
            className="flex-row items-center justify-between bg-white dark:bg-gray-800 rounded-xl p-4 mb-2"
            accessibilityRole="button"
            accessibilityLabel="Tənzimləmələr"
          >
            <View className="flex-row items-center">
              <Text className="text-base mr-3">{'⚙'}</Text>
              <Text className="text-base text-primary dark:text-white">
                Tənzimləmələr
              </Text>
            </View>
            <Text className="text-gray-400">{'>'}</Text>
          </Pressable>

          <Pressable
            className="flex-row items-center justify-between bg-white dark:bg-gray-800 rounded-xl p-4 mb-2"
            accessibilityRole="button"
            accessibilityLabel="Məxfilik siyasəti"
          >
            <View className="flex-row items-center">
              <Text className="text-base mr-3">📄</Text>
              <Text className="text-base text-primary dark:text-white">
                Məxfilik siyasəti
              </Text>
            </View>
            <Text className="text-gray-400">{'>'}</Text>
          </Pressable>
        </View>

        {/* Logout */}
        <View className="px-6">
          <Pressable
            onPress={logout}
            className="bg-error rounded-xl py-3 items-center"
            accessibilityRole="button"
            accessibilityLabel="Çıxış"
          >
            <Text className="text-white font-semibold">Çıxış</Text>
          </Pressable>
        </View>
      </ScrollView>
    </View>
  );
}
