import { View, Text } from 'react-native';

interface Props {
  icon?: string;
  message: string;
}

export function EmptyState({ message }: Props) {
  return (
    <View className="flex-1 items-center justify-center px-8 bg-background-light dark:bg-background-dark">
      <Text className="text-5xl mb-4">O</Text>
      <Text className="text-lg font-semibold text-primary dark:text-white text-center">
        {message}
      </Text>
    </View>
  );
}
