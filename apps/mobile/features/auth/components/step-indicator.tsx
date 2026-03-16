import { View } from 'react-native';

interface Props {
  totalSteps: number;
  currentStep: number;
}

export function StepIndicator({ totalSteps, currentStep }: Props) {
  return (
    <View className="flex-row items-center justify-center gap-2 py-4">
      {Array.from({ length: totalSteps }).map((_, i) => (
        <View
          key={i}
          className={`h-2 rounded-full ${
            i <= currentStep
              ? 'w-8 bg-accent'
              : 'w-2 bg-gray-300 dark:bg-gray-600'
          }`}
        />
      ))}
    </View>
  );
}
