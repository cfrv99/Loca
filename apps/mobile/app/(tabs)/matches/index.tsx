import { View, Text } from 'react-native';

export default function MatchesScreen() {
  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center">
      <Text className="text-2xl font-bold text-primary dark:text-white mb-2">
        Maclar
      </Text>
      <Text className="text-sm text-gray-500 text-center px-8">
        Henuez mac yoxdur. Bir mekanda birini begenin ve mac gonderin!
      </Text>
    </View>
  );
}
