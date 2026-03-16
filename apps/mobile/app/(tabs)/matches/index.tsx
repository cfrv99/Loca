import { View, Text } from 'react-native';
import { EmptyState } from '../../../shared/components/empty-state';

export default function MatchesScreen() {
  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <View className="px-4 pt-12 pb-2">
        <Text className="text-3xl font-bold text-primary dark:text-white">
          Matçlar
        </Text>
      </View>
      <EmptyState
        icon="💬"
        message="Hələ heç bir matçınız yoxdur"
      />
    </View>
  );
}
