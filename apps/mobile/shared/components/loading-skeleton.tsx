import { View } from 'react-native';

interface Props {
  variant?: 'venue-list' | 'chat' | 'profile' | 'default';
}

export function LoadingSkeleton({ variant = 'default' }: Props) {
  const count = variant === 'venue-list' ? 4 : variant === 'chat' ? 8 : 3;

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark p-4">
      {Array.from({ length: count }).map((_, i) => (
        <View
          key={i}
          className="bg-gray-200 dark:bg-gray-700 rounded-2xl mb-3 animate-pulse"
          style={{ height: variant === 'venue-list' ? 200 : variant === 'chat' ? 48 : 80 }}
        />
      ))}
    </View>
  );
}
