import { useState } from 'react';
import { View, Text, Pressable, TextInput, Alert } from 'react-native';
import { api } from '../../../shared/services/api-client';
import type { ReportReason } from '../../../shared/types';

interface Props {
  userId: string;
  onClose: () => void;
  onBlock: () => void;
}

const REASONS: { value: ReportReason; label: string }[] = [
  { value: 'harassment', label: 'Təhqir' },
  { value: 'spam', label: 'Spam' },
  { value: 'fake', label: 'Saxta profil' },
  { value: 'inappropriate', label: 'Uyğunsuz məzmun' },
  { value: 'other', label: 'Digər' },
];

export function ReportSheet({ userId, onClose, onBlock }: Props) {
  const [selectedReason, setSelectedReason] = useState<ReportReason | null>(
    null
  );
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (!selectedReason) return;

    setIsSubmitting(true);
    try {
      await api.post(`/users/${userId}/report`, {
        reason: selectedReason,
        description: description.trim() || undefined,
      });
      Alert.alert('Göndərildi', 'Şikayətiniz qeydə alındı');
      onClose();
    } catch {
      Alert.alert('Xəta', 'Şikayət göndərilmədi');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <View className="absolute inset-0 bg-black/50 justify-end">
      <Pressable className="flex-1" onPress={onClose} accessibilityLabel="Bağla" />

      <View className="bg-white dark:bg-gray-800 rounded-t-3xl px-6 pt-6 pb-10">
        {/* Handle */}
        <View className="w-10 h-1 bg-gray-300 dark:bg-gray-600 rounded-full self-center mb-4" />

        <Text className="text-xl font-bold text-primary dark:text-white mb-4">
          Şikayət et
        </Text>

        {/* Reasons */}
        {REASONS.map((reason) => (
          <Pressable
            key={reason.value}
            onPress={() => setSelectedReason(reason.value)}
            className={`flex-row items-center py-3 px-4 rounded-xl mb-2 border ${
              selectedReason === reason.value
                ? 'border-accent bg-accent/10'
                : 'border-gray-200 dark:border-gray-700'
            }`}
            accessibilityRole="radio"
            accessibilityLabel={reason.label}
            accessibilityState={{ selected: selectedReason === reason.value }}
          >
            <View
              className={`w-5 h-5 rounded-full border-2 mr-3 items-center justify-center ${
                selectedReason === reason.value
                  ? 'border-accent'
                  : 'border-gray-300 dark:border-gray-600'
              }`}
            >
              {selectedReason === reason.value && (
                <View className="w-3 h-3 rounded-full bg-accent" />
              )}
            </View>
            <Text className="text-base text-primary dark:text-white">
              {reason.label}
            </Text>
          </Pressable>
        ))}

        {/* Description */}
        <TextInput
          className="bg-gray-100 dark:bg-gray-700 rounded-xl py-3 px-4 text-base text-primary dark:text-white mt-3 min-h-[80px]"
          placeholder="Əlavə məlumat (istəyə bağlı)"
          placeholderTextColor="#9CA3AF"
          value={description}
          onChangeText={setDescription}
          multiline
          maxLength={500}
          accessibilityLabel="Əlavə məlumat"
        />

        {/* Actions */}
        <View className="flex-row gap-3 mt-4">
          <Pressable
            onPress={onBlock}
            className="flex-1 border border-error rounded-xl py-3 items-center"
            accessibilityRole="button"
            accessibilityLabel="Blokla"
          >
            <Text className="text-error font-semibold">Blokla</Text>
          </Pressable>
          <Pressable
            onPress={handleSubmit}
            disabled={!selectedReason || isSubmitting}
            className={`flex-1 rounded-xl py-3 items-center ${
              selectedReason && !isSubmitting
                ? 'bg-accent'
                : 'bg-gray-300 dark:bg-gray-600'
            }`}
            accessibilityRole="button"
            accessibilityLabel="Şikayəti göndər"
          >
            <Text className="text-white font-semibold">
              {isSubmitting ? 'Yüklənir...' : 'Göndər'}
            </Text>
          </Pressable>
        </View>

        {/* Cancel */}
        <Pressable
          onPress={onClose}
          className="mt-3 py-3 items-center"
          accessibilityRole="button"
          accessibilityLabel="Ləğv et"
        >
          <Text className="text-gray-500 text-base">Ləğv et</Text>
        </Pressable>
      </View>
    </View>
  );
}
