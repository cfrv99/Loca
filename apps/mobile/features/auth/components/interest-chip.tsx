import { Pressable, Text } from 'react-native';

interface Props {
  label: string;
  isSelected: boolean;
  onToggle: () => void;
}

export function InterestChip({ label, isSelected, onToggle }: Props) {
  return (
    <Pressable
      onPress={onToggle}
      className={`rounded-full px-4 py-2 mr-2 mb-2 border ${
        isSelected
          ? 'bg-accent border-accent'
          : 'bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-600'
      }`}
      accessibilityRole="button"
      accessibilityLabel={`${label} ${isSelected ? 'secildi' : 'secilmeyib'}`}
      accessibilityState={{ selected: isSelected }}
    >
      <Text
        className={`text-sm font-medium ${
          isSelected ? 'text-white' : 'text-primary dark:text-white'
        }`}
      >
        {label}
      </Text>
    </Pressable>
  );
}
