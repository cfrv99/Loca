import { View, Text, Pressable } from 'react-native';
import { useAuthStore } from '@/shared/stores/auth-store';

export default function ProfileScreen() {
  const { user, clearAuth } = useAuthStore();

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark pt-14 px-4">
      <Text className="text-3xl font-bold text-primary dark:text-white mb-6">
        Profil
      </Text>

      <View className="bg-white dark:bg-gray-800 rounded-2xl p-6 mb-4">
        <View className="items-center mb-4">
          <View className="w-24 h-24 rounded-full bg-accent items-center justify-center mb-3">
            <Text className="text-3xl font-bold text-white">
              {user?.displayName?.[0] ?? 'U'}
            </Text>
          </View>
          <Text className="text-xl font-semibold text-primary dark:text-white">
            {user?.displayName ?? 'Istifadeci'}
          </Text>
          <Text className="text-sm text-gray-500">{user?.email ?? ''}</Text>
        </View>
      </View>

      <Pressable
        onPress={clearAuth}
        className="bg-error rounded-xl py-3 px-6 items-center active:opacity-80"
        accessibilityRole="button"
        accessibilityLabel="Cixis et"
      >
        <Text className="text-white font-semibold text-base">Cixis et</Text>
      </Pressable>
    </View>
  );
}
