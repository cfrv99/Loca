import { useState, useCallback } from 'react';
import {
  View,
  Text,
  TextInput,
  Pressable,
  ScrollView,
  Image,
  Alert,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '../../../shared/stores/auth-store';
import { api } from '../../../shared/services/api-client';
import { InterestChip } from '../../../features/auth/components/interest-chip';
import { VibeSlider } from '../../../features/auth/components/vibe-slider';
import type { ApiResponse, UserDto, VibePreferenceDto } from '../../../shared/types';

const INTERESTS = [
  'Musiqi',
  'Yemək',
  'Sport',
  'Səyahət',
  'Texnologiya',
  'Sənət',
  'Gecə Həyatı',
  'İş/Networking',
];

const PURPOSES = ['Tanışlıq', 'Dostluq', 'Networking', 'Əyləncə'];

const GENDERS = [
  { value: 'male', label: 'Kişi' },
  { value: 'female', label: 'Qadın' },
  { value: 'other', label: 'Digər' },
  { value: 'prefer_not_to_say', label: 'Demək istəmirəm' },
];

export default function ProfileEditScreen() {
  const router = useRouter();
  const { user, setUser } = useAuthStore();

  const [isSaving, setIsSaving] = useState(false);
  const [displayName, setDisplayName] = useState(user?.displayName ?? '');
  const [gender, setGender] = useState(user?.gender ?? '');
  const [interests, setInterests] = useState<string[]>(user?.interests ?? []);
  const [purposes, setPurposes] = useState<string[]>(user?.purposes ?? []);

  const vibeMap = (user?.vibePreferences ?? []).reduce(
    (acc, v) => {
      acc[v.vibe] = Math.round(v.weight * 100);
      return acc;
    },
    {} as Record<string, number>
  );

  const [vibes, setVibes] = useState({
    Romantic: vibeMap['Romantic'] ?? 50,
    Party: vibeMap['Party'] ?? 50,
    Chill: vibeMap['Chill'] ?? 50,
    Adventurous: vibeMap['Adventurous'] ?? 50,
  });

  const toggleInterest = useCallback((interest: string) => {
    setInterests((prev) =>
      prev.includes(interest)
        ? prev.filter((i) => i !== interest)
        : [...prev, interest]
    );
  }, []);

  const togglePurpose = useCallback((purpose: string) => {
    setPurposes((prev) =>
      prev.includes(purpose)
        ? prev.filter((p) => p !== purpose)
        : [...prev, purpose]
    );
  }, []);

  const handleSave = async () => {
    if (displayName.trim().length < 2) {
      Alert.alert('Xəta', 'Ad ən azı 2 simvol olmalıdır');
      return;
    }

    setIsSaving(true);
    try {
      const vibePreferences: VibePreferenceDto[] = Object.entries(vibes).map(
        ([vibe, weight]) => ({
          vibe,
          weight: weight / 100,
        })
      );

      const res = await api.put<ApiResponse<UserDto>>('/users/me', {
        displayName: displayName.trim(),
        gender,
        interests,
        purposes,
        vibePreferences,
      });

      if (res.data.success && res.data.data) {
        setUser(res.data.data);
        router.back();
      } else {
        Alert.alert('Xəta', res.data.error?.message ?? 'Xəta baş verdi');
      }
    } catch {
      Alert.alert('Xəta', 'Profil yadda saxlanmadı');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="flex-row items-center justify-between px-4 pt-12 pb-4">
        <Pressable
          onPress={() => router.back()}
          className="py-2 pr-4"
          accessibilityRole="button"
          accessibilityLabel="Geri"
        >
          <Text className="text-accent text-base font-medium">Geri</Text>
        </Pressable>
        <Text className="text-lg font-semibold text-primary dark:text-white">
          Profili Redaktə Et
        </Text>
        <Pressable
          onPress={handleSave}
          disabled={isSaving}
          className="py-2 pl-4"
          accessibilityRole="button"
          accessibilityLabel="Saxla"
        >
          <Text
            className={`text-base font-semibold ${
              isSaving ? 'text-gray-400' : 'text-accent'
            }`}
          >
            {isSaving ? 'Yüklənir...' : 'Saxla'}
          </Text>
        </Pressable>
      </View>

      <ScrollView
        className="flex-1"
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 40 }}
      >
        {/* Avatar */}
        <View className="items-center mb-6">
          <Pressable
            className="w-24 h-24 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center"
            accessibilityRole="button"
            accessibilityLabel="Profil foto dəyiş"
          >
            {user?.avatarUrl ? (
              <Image
                source={{ uri: user.avatarUrl }}
                className="w-24 h-24 rounded-full"
                accessibilityLabel="Profil foto"
              />
            ) : (
              <Text className="text-3xl">📷</Text>
            )}
          </Pressable>
          <Text className="text-xs text-accent mt-2">Fotonu dəyiş</Text>
        </View>

        {/* Display Name */}
        <View className="px-6 mb-6">
          <Text className="text-sm font-medium text-primary dark:text-white mb-1">
            Ad
          </Text>
          <TextInput
            className="bg-gray-100 dark:bg-gray-700 rounded-xl py-3 px-4 text-base text-primary dark:text-white"
            placeholder="Adınız"
            placeholderTextColor="#9CA3AF"
            value={displayName}
            onChangeText={setDisplayName}
            maxLength={100}
            accessibilityLabel="Ad"
          />
        </View>

        {/* Gender */}
        <View className="px-6 mb-6">
          <Text className="text-sm font-medium text-primary dark:text-white mb-2">
            Cins
          </Text>
          <View className="flex-row flex-wrap">
            {GENDERS.map((g) => (
              <Pressable
                key={g.value}
                onPress={() => setGender(g.value)}
                className={`rounded-full px-4 py-2 mr-2 mb-2 border ${
                  gender === g.value
                    ? 'bg-accent border-accent'
                    : 'bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-600'
                }`}
                accessibilityRole="radio"
                accessibilityLabel={g.label}
                accessibilityState={{ selected: gender === g.value }}
              >
                <Text
                  className={`text-sm font-medium ${
                    gender === g.value
                      ? 'text-white'
                      : 'text-primary dark:text-white'
                  }`}
                >
                  {g.label}
                </Text>
              </Pressable>
            ))}
          </View>
        </View>

        {/* Interests */}
        <View className="px-6 mb-6">
          <Text className="text-sm font-medium text-primary dark:text-white mb-2">
            Maraqlar
          </Text>
          <View className="flex-row flex-wrap">
            {INTERESTS.map((interest) => (
              <InterestChip
                key={interest}
                label={interest}
                isSelected={interests.includes(interest)}
                onToggle={() => toggleInterest(interest)}
              />
            ))}
          </View>
        </View>

        {/* Purposes */}
        <View className="px-6 mb-6">
          <Text className="text-sm font-medium text-primary dark:text-white mb-2">
            Məqsəd
          </Text>
          <View className="flex-row flex-wrap">
            {PURPOSES.map((purpose) => (
              <InterestChip
                key={purpose}
                label={purpose}
                isSelected={purposes.includes(purpose)}
                onToggle={() => togglePurpose(purpose)}
              />
            ))}
          </View>
        </View>

        {/* Vibe Sliders */}
        <View className="px-6 mb-6">
          <Text className="text-sm font-medium text-primary dark:text-white mb-4">
            Vibe Seçimləri
          </Text>
          <VibeSlider
            label="Romantik"
            value={vibes.Romantic}
            onValueChange={(v) =>
              setVibes((prev) => ({ ...prev, Romantic: v }))
            }
          />
          <VibeSlider
            label="Party"
            value={vibes.Party}
            onValueChange={(v) =>
              setVibes((prev) => ({ ...prev, Party: v }))
            }
          />
          <VibeSlider
            label="Sakit"
            value={vibes.Chill}
            onValueChange={(v) =>
              setVibes((prev) => ({ ...prev, Chill: v }))
            }
          />
          <VibeSlider
            label="Macəraperest"
            value={vibes.Adventurous}
            onValueChange={(v) =>
              setVibes((prev) => ({ ...prev, Adventurous: v }))
            }
          />
        </View>
      </ScrollView>
    </View>
  );
}
