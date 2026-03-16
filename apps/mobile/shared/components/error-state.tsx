import { View, Text, Pressable } from 'react-native';

interface Props {
  message: string;
  onRetry?: () => void;
}

export function ErrorState({ message, onRetry }: Props) {
  return (
    <View className="flex-1 items-center justify-center px-8 bg-background-light dark:bg-background-dark">
      <Text className="text-4xl mb-4">!</Text>
      <Text className="text-lg font-semibold text-primary dark:text-white text-center mb-2">
        {message}
      </Text>
      <Text className="text-sm text-gray-500 text-center mb-6">
        Zehmet olmasa yeniden cehd edin
      </Text>
      {onRetry && (
        <Pressable
          onPress={onRetry}
          className="bg-accent rounded-xl py-3 px-8 active:opacity-80"
          accessibilityRole="button"
          accessibilityLabel="Yeniden cehd et"
        >
          <Text className="text-white font-semibold text-base">
            Yeniden cehd et
          </Text>
        </Pressable>
      )}
    </View>
  );
}
