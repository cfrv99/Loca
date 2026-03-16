import { View, TextInput, Pressable, Text } from 'react-native';
import { useState } from 'react';
import { CONFIG } from '../../../shared/constants/config';

interface Props {
  onSend: (content: string, type?: string) => void;
  onTyping?: () => void;
  onStopTyping?: () => void;
  onGiftPress?: () => void;
  isConnected: boolean;
  placeholder?: string;
}

export function MessageInput({
  onSend,
  onTyping,
  onStopTyping,
  onGiftPress,
  isConnected,
  placeholder = 'Mesaj yaz...',
}: Props) {
  const [input, setInput] = useState('');

  const handleSend = () => {
    const trimmed = input.trim();
    if (!trimmed) return;
    onSend(trimmed, 'text');
    setInput('');
    onStopTyping?.();
  };

  const handleChangeText = (text: string) => {
    setInput(text);
    if (text.length > 0) {
      onTyping?.();
    } else {
      onStopTyping?.();
    }
  };

  return (
    <View className="flex-row items-end px-4 py-2 bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
      {/* Gift button */}
      {onGiftPress && (
        <Pressable
          onPress={onGiftPress}
          disabled={!isConnected}
          className="w-10 h-10 items-center justify-center mr-1"
          accessibilityLabel="Hediyye gonder"
          accessibilityRole="button"
        >
          <Text className="text-xl">🎁</Text>
        </Pressable>
      )}

      {/* Text input */}
      <TextInput
        className="flex-1 bg-gray-100 dark:bg-gray-700 rounded-2xl px-4 py-2 text-base text-primary dark:text-white max-h-24"
        placeholder={placeholder}
        placeholderTextColor="#9CA3AF"
        value={input}
        onChangeText={handleChangeText}
        multiline
        maxLength={CONFIG.MESSAGE_MAX_LENGTH}
        editable={isConnected}
        accessibilityLabel={placeholder}
      />

      {/* Send button */}
      <Pressable
        onPress={handleSend}
        disabled={!input.trim() || !isConnected}
        className={`ml-2 rounded-full w-10 h-10 items-center justify-center ${
          input.trim() && isConnected ? 'bg-accent' : 'bg-gray-300 dark:bg-gray-600'
        }`}
        accessibilityLabel="Mesaj gonder"
        accessibilityRole="button"
      >
        <Text className="text-white text-lg font-bold">{'>'}</Text>
      </Pressable>
    </View>
  );
}
