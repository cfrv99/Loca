import { View, Text, Pressable } from 'react-native';
import { useAuthStore } from '../../../shared/stores/auth-store';

export default function ProfileScreen() {
  const { user, logout } = useAuthStore();

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <View className="px-4 pt-12 pb-4">
        <Text className="text-3xl font-bold text-primary dark:text-white">
          Profil
        </Text>
      </View>

      <View className="px-4">
        {/* Avatar placeholder */}
        <View className="w-24 h-24 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center self-center mb-4">
          <Text className="text-4xl">👤</Text>
        </View>

        <Text className="text-xl font-semibold text-center text-primary dark:text-white mb-1">
          {user?.displayName ?? 'İstifadəçi'}
        </Text>
        <Text className="text-sm text-gray-500 text-center mb-6">
          {user?.email ?? ''}
        </Text>

        {/* Stats */}
        <View className="flex-row justify-around mb-8">
          <View className="items-center">
            <Text className="text-xl font-bold text-accent">{user?.coinBalance ?? 0}</Text>
            <Text className="text-xs text-gray-500">Coin</Text>
          </View>
          <View className="items-center">
            <Text className="text-xl font-bold text-accent">{user?.interests?.length ?? 0}</Text>
            <Text className="text-xs text-gray-500">Maraqlar</Text>
          </View>
        </View>

        {/* Logout */}
        <Pressable
          onPress={logout}
          className="bg-error rounded-xl py-3 px-6 items-center"
          accessibilityRole="button"
          accessibilityLabel="Çıxış"
        >
          <Text className="text-white font-semibold">Çıxış</Text>
        </Pressable>
      </View>
    </View>
  );
}
