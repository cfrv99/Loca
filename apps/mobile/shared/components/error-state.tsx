import { View, Text, Pressable } from 'react-native';

interface Props {
  message?: string;
  onRetry?: () => void;
}

export function ErrorState({ message = 'Xəta baş verdi', onRetry }: Props) {
  return (
    <View className="flex-1 items-center justify-center bg-background-light dark:bg-background-dark p-8">
      <Text className="text-4xl mb-4">😕</Text>
      <Text className="text-lg font-semibold text-primary dark:text-white text-center mb-2">
        {message}
      </Text>
      {onRetry && (
        <Pressable
          onPress={onRetry}
          className="bg-accent rounded-xl py-3 px-6 mt-4"
          accessibilityRole="button"
          accessibilityLabel="Yenidən cəhd et"
        >
          <Text className="text-white font-semibold">Yenidən cəhd et</Text>
        </Pressable>
      )}
    </View>
  );
}
