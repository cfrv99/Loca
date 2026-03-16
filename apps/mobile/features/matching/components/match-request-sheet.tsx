import { useState } from 'react';
import { View, Text, Pressable, TextInput, Alert } from 'react-native';
import { api } from '../../../shared/services/api-client';
import type { ApiResponse, MatchRequestDto } from '../../../shared/types';

interface Props {
  receiverId: string;
  venueId: string;
  onClose: () => void;
  onSuccess: () => void;
}

export function MatchRequestSheet({
  receiverId,
  venueId,
  onClose,
  onSuccess,
}: Props) {
  const [introMessage, setIntroMessage] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSend = async () => {
    setIsSubmitting(true);
    try {
      const res = await api.post<ApiResponse<MatchRequestDto>>(
        '/matches/request',
        {
          receiverId,
          introMessage: introMessage.trim() || undefined,
        }
      );

      if (res.data.success) {
        onSuccess();
      } else {
        const code = res.data.error?.code ?? '';
        let msg = res.data.error?.message ?? 'Xəta baş verdi';

        switch (code) {
          case 'SAME_VENUE_REQUIRED':
            msg = 'Hər iki istifadəçi eyni məkanda olmalıdır';
            break;
          case 'ANONYMOUS_NOT_ALLOWED':
            msg = 'Anonim istifadəçilər sorğu göndərə bilməz';
            break;
          case 'DAILY_LIMIT_REACHED':
            msg = 'Gündəlik limit dolub (5 sorğu)';
            break;
          case 'USER_BLOCKED':
            msg = 'Bu istifadəçi bloklanıb';
            break;
          case 'ALREADY_PENDING':
            msg = 'Artıq gözləyən sorğunuz var';
            break;
        }

        Alert.alert('Xəta', msg);
      }
    } catch {
      Alert.alert('Xəta', 'Sorğu göndərilmədi');
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

        <Text className="text-xl font-bold text-primary dark:text-white mb-2">
          Tanışlıq sorğusu
        </Text>
        <Text className="text-sm text-gray-500 dark:text-gray-400 mb-4">
          Qısa bir mesaj yazaraq özünüzü tanıdın (istəyə bağlı)
        </Text>

        {/* Intro Message */}
        <TextInput
          className="bg-gray-100 dark:bg-gray-700 rounded-xl py-3 px-4 text-base text-primary dark:text-white min-h-[100px]"
          placeholder="Salam! Mən..."
          placeholderTextColor="#9CA3AF"
          value={introMessage}
          onChangeText={setIntroMessage}
          multiline
          maxLength={200}
          accessibilityLabel="Tanışlıq mesajı"
        />
        <Text className="text-xs text-gray-400 text-right mt-1">
          {introMessage.length}/200
        </Text>

        {/* Send Button */}
        <Pressable
          onPress={handleSend}
          disabled={isSubmitting}
          className={`rounded-xl py-3 items-center mt-4 ${
            isSubmitting ? 'bg-gray-300 dark:bg-gray-600' : 'bg-accent'
          }`}
          accessibilityRole="button"
          accessibilityLabel="Sorğu göndər"
        >
          <Text className="text-white font-semibold text-base">
            {isSubmitting ? 'Göndərilir...' : 'Göndər'}
          </Text>
        </Pressable>

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
