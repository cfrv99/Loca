import { View, Text, Pressable } from 'react-native';

interface Props {
  label: string;
  value: number;
  onValueChange: (value: number) => void;
}

export function VibeSlider({ label, value, onValueChange }: Props) {
  const steps = [0, 25, 50, 75, 100];

  return (
    <View className="mb-6">
      <View className="flex-row items-center justify-between mb-2">
        <Text className="text-base font-medium text-primary dark:text-white">
          {label}
        </Text>
        <Text className="text-sm font-semibold text-accent">{value}%</Text>
      </View>
      <View className="flex-row items-center gap-1">
        {steps.map((step) => (
          <Pressable
            key={step}
            onPress={() => onValueChange(step)}
            className="flex-1"
            accessibilityRole="adjustable"
            accessibilityLabel={`${label} ${step} faiz`}
          >
            <View
              className={`h-3 rounded-full ${
                step <= value ? 'bg-accent' : 'bg-gray-200 dark:bg-gray-700'
              }`}
            />
          </Pressable>
        ))}
      </View>
    </View>
  );
}
