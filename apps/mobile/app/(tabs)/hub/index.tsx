import { View, Text } from 'react-native';

export default function HubScreen() {
  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center">
      <Text className="text-2xl font-bold text-primary dark:text-white mb-2">
        Social Hub
      </Text>
      <Text className="text-sm text-gray-500 text-center px-8">
        Bir mekana check-in edin. Chat, oyunlar ve insanlar burada gorunecek.
      </Text>
    </View>
  );
}
