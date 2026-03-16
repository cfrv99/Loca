import { View, Text } from 'react-native';

interface Props {
  icon?: string;
  message?: string;
}

export function EmptyState({ icon = '📭', message = 'Nəticə tapılmadı' }: Props) {
  return (
    <View className="flex-1 items-center justify-center bg-background-light dark:bg-background-dark p-8">
      <Text className="text-5xl mb-4">{icon}</Text>
      <Text className="text-lg text-gray-500 dark:text-gray-400 text-center">
        {message}
      </Text>
    </View>
  );
}
