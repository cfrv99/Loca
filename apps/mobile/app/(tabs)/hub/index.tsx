import { View, Text } from 'react-native';
import { EmptyState } from '../../../shared/components/empty-state';

export default function HubScreen() {
  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <View className="px-4 pt-12 pb-2">
        <Text className="text-3xl font-bold text-primary dark:text-white">
          Hub
        </Text>
      </View>
      <EmptyState
        icon="📱"
        message="QR scan edərək bir məkana qoşulun"
      />
    </View>
  );
}
