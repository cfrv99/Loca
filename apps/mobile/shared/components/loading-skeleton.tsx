import { View } from 'react-native';

interface Props {
  variant?: 'venue-list' | 'chat' | 'profile' | 'game';
}

export function LoadingSkeleton({ variant = 'venue-list' }: Props) {
  const renderSkeletonItem = (index: number) => (
    <View
      key={index}
      className="mx-4 mb-3 bg-gray-200 dark:bg-gray-700 rounded-2xl overflow-hidden"
    >
      <View className="w-full h-40 bg-gray-300 dark:bg-gray-600" />
      <View className="p-4">
        <View className="h-5 w-3/4 bg-gray-300 dark:bg-gray-600 rounded mb-2" />
        <View className="h-4 w-1/2 bg-gray-300 dark:bg-gray-600 rounded mb-2" />
        <View className="h-3 w-1/3 bg-gray-300 dark:bg-gray-600 rounded" />
      </View>
    </View>
  );

  const count = variant === 'venue-list' ? 4 : variant === 'chat' ? 8 : 3;

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark pt-4">
      {Array.from({ length: count }, (_, i) => renderSkeletonItem(i))}
    </View>
  );
}
