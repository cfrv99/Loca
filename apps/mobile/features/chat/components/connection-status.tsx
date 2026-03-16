import { View, Text } from 'react-native';

interface Props {
  isConnected: boolean;
}

export function ConnectionStatus({ isConnected }: Props) {
  if (isConnected) return null;

  return (
    <View
      className="bg-warning px-4 py-1.5"
      accessibilityLabel="Baglanti berpa edilir"
      accessibilityRole="alert"
    >
      <Text className="text-xs text-center text-white font-medium">
        Baglanti berpa edilir...
      </Text>
    </View>
  );
}
