import { View, Pressable, Text } from 'react-native';

const REACTIONS = [
  { emoji: '\u2764\uFE0F', label: 'urek' },
  { emoji: '\uD83D\uDE02', label: 'gulush' },
  { emoji: '\uD83D\uDE2E', label: 'heyret' },
  { emoji: '\uD83D\uDC4F', label: 'alqis' },
  { emoji: '\uD83D\uDD25', label: 'od' },
  { emoji: '\uD83D\uDE22', label: 'kederlenmek' },
];

interface Props {
  onSelect: (emoji: string) => void;
  onClose: () => void;
}

export function ReactionPicker({ onSelect, onClose }: Props) {
  return (
    <Pressable
      onPress={onClose}
      className="absolute -top-12 left-0 z-50"
      accessibilityLabel="Reaksiya sec"
      accessibilityRole="menu"
    >
      <View className="flex-row bg-white dark:bg-gray-800 rounded-full shadow-lg px-2 py-1.5 gap-1">
        {REACTIONS.map((reaction) => (
          <Pressable
            key={reaction.emoji}
            onPress={() => onSelect(reaction.emoji)}
            className="w-9 h-9 items-center justify-center rounded-full active:bg-gray-100 dark:active:bg-gray-700"
            accessibilityLabel={reaction.label}
            accessibilityRole="button"
          >
            <Text className="text-xl">{reaction.emoji}</Text>
          </Pressable>
        ))}
      </View>
    </Pressable>
  );
}
