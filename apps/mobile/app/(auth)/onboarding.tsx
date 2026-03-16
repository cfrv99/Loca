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
import { useAuthStore } from '../../shared/stores/auth-store';
import { api } from '../../shared/services/api-client';
import { StepIndicator } from '../../features/auth/components/step-indicator';
import { InterestChip } from '../../features/auth/components/interest-chip';
import { VibeSlider } from '../../features/auth/components/vibe-slider';
import type { ApiResponse, UserDto, VibePreferenceDto } from '../../shared/types';

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

const TOTAL_STEPS = 5;

export default function OnboardingScreen() {
  const router = useRouter();
  const { user, setUser } = useAuthStore();

  const [currentStep, setCurrentStep] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Step 1: Basic Info
  const [displayName, setDisplayName] = useState(user?.displayName ?? '');
  const [gender, setGender] = useState(user?.gender ?? '');
  const [avatarUri, setAvatarUri] = useState<string | undefined>(
    user?.avatarUrl ?? undefined
  );

  // Step 2: Interests
  const [interests, setInterests] = useState<string[]>(user?.interests ?? []);

  // Step 3: Purposes
  const [purposes, setPurposes] = useState<string[]>(user?.purposes ?? []);

  // Step 4: Vibe
  const [vibes, setVibes] = useState({
    Romantic: 50,
    Party: 50,
    Chill: 50,
    Adventurous: 50,
  });

  // Step 5: Privacy
  const [defaultAnonymous, setDefaultAnonymous] = useState(false);
  const [pushEnabled, setPushEnabled] = useState(true);

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

  const canProceed = (): boolean => {
    switch (currentStep) {
      case 0:
        return displayName.trim().length >= 2 && gender.length > 0;
      case 1:
        return interests.length >= 1;
      case 2:
        return purposes.length >= 1;
      case 3:
        return true;
      case 4:
        return true;
      default:
        return false;
    }
  };

  const handleNext = useCallback(() => {
    if (currentStep < TOTAL_STEPS - 1) {
      setCurrentStep((prev) => prev + 1);
    } else {
      handleSubmit();
    }
  }, [currentStep]);

  const handleBack = useCallback(() => {
    if (currentStep > 0) {
      setCurrentStep((prev) => prev - 1);
    }
  }, [currentStep]);

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      const vibePreferences: VibePreferenceDto[] = Object.entries(vibes).map(
        ([vibe, weight]) => ({
          vibe,
          weight: weight / 100,
        })
      );

      const res = await api.put<ApiResponse<UserDto>>('/users/me/onboarding', {
        displayName: displayName.trim(),
        gender,
        interests,
        purposes,
        vibePreferences,
        privacySettings: {
          defaultAnonymous,
          pushEnabled,
        },
      });

      if (res.data.success && res.data.data) {
        setUser(res.data.data);
        router.replace('/(tabs)/discover');
      } else {
        Alert.alert('Xəta', res.data.error?.message ?? 'Xəta baş verdi');
      }
    } catch {
      Alert.alert('Xəta', 'Profil yadda saxlanmadı. Yenidən cəhd edin.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderStep = () => {
    switch (currentStep) {
      case 0:
        return (
          <View className="px-6">
            <Text className="text-xl font-semibold text-primary dark:text-white mb-2">
              Əsas Məlumatlar
            </Text>
            <Text className="text-sm text-gray-500 dark:text-gray-400 mb-6">
              Digər istifadəçilərin sizi tanıması üçün
            </Text>

            {/* Avatar */}
            <Pressable
              className="w-24 h-24 rounded-full bg-gray-200 dark:bg-gray-700 items-center justify-center self-center mb-6"
              accessibilityRole="button"
              accessibilityLabel="Profil foto seç"
            >
              {avatarUri ? (
                <Image
                  source={{ uri: avatarUri }}
                  className="w-24 h-24 rounded-full"
                  accessibilityLabel="Profil foto"
                />
              ) : (
                <Text className="text-3xl">📷</Text>
              )}
            </Pressable>

            {/* Display Name */}
            <Text className="text-sm font-medium text-primary dark:text-white mb-1">
              Ad
            </Text>
            <TextInput
              className="bg-gray-100 dark:bg-gray-700 rounded-xl py-3 px-4 text-base text-primary dark:text-white mb-4"
              placeholder="Adınızı daxil edin"
              placeholderTextColor="#9CA3AF"
              value={displayName}
              onChangeText={setDisplayName}
              maxLength={100}
              accessibilityLabel="Ad daxil edin"
            />

            {/* Gender */}
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
        );

      case 1:
        return (
          <View className="px-6">
            <Text className="text-xl font-semibold text-primary dark:text-white mb-2">
              Maraqlar
            </Text>
            <Text className="text-sm text-gray-500 dark:text-gray-400 mb-6">
              Nə ilə maraqlanırsınız? (ən azı 1 seçin)
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
        );

      case 2:
        return (
          <View className="px-6">
            <Text className="text-xl font-semibold text-primary dark:text-white mb-2">
              Məqsəd
            </Text>
            <Text className="text-sm text-gray-500 dark:text-gray-400 mb-6">
              Loca-da nə axtarırsınız? (ən azı 1 seçin)
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
        );

      case 3:
        return (
          <View className="px-6">
            <Text className="text-xl font-semibold text-primary dark:text-white mb-2">
              Vibe Seçimləri
            </Text>
            <Text className="text-sm text-gray-500 dark:text-gray-400 mb-6">
              Hansı atmosferi sevdiyinizi seçin
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
        );

      case 4:
        return (
          <View className="px-6">
            <Text className="text-xl font-semibold text-primary dark:text-white mb-2">
              Gizlilik
            </Text>
            <Text className="text-sm text-gray-500 dark:text-gray-400 mb-6">
              Gizlilik parametrlərinizi seçin
            </Text>

            {/* Anonymous Toggle */}
            <Pressable
              onPress={() => setDefaultAnonymous((prev) => !prev)}
              className="flex-row items-center justify-between bg-white dark:bg-gray-800 rounded-xl p-4 mb-3"
              accessibilityRole="switch"
              accessibilityLabel="Anonim rejim"
              accessibilityState={{ checked: defaultAnonymous }}
            >
              <View className="flex-1 mr-4">
                <Text className="text-base font-medium text-primary dark:text-white">
                  Anonim rejim
                </Text>
                <Text className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Check-in zamanı adınız gizli olsun
                </Text>
              </View>
              <View
                className={`w-12 h-7 rounded-full items-center justify-center ${
                  defaultAnonymous ? 'bg-accent' : 'bg-gray-300 dark:bg-gray-600'
                }`}
              >
                <View
                  className={`w-5 h-5 rounded-full bg-white ${
                    defaultAnonymous ? 'ml-5' : 'mr-5'
                  }`}
                />
              </View>
            </Pressable>

            {/* Push Notifications Toggle */}
            <Pressable
              onPress={() => setPushEnabled((prev) => !prev)}
              className="flex-row items-center justify-between bg-white dark:bg-gray-800 rounded-xl p-4 mb-3"
              accessibilityRole="switch"
              accessibilityLabel="Push bildirisleri"
              accessibilityState={{ checked: pushEnabled }}
            >
              <View className="flex-1 mr-4">
                <Text className="text-base font-medium text-primary dark:text-white">
                  Push bildirişləri
                </Text>
                <Text className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Yeni mesaj, matç və oyun bildirişləri
                </Text>
              </View>
              <View
                className={`w-12 h-7 rounded-full items-center justify-center ${
                  pushEnabled ? 'bg-accent' : 'bg-gray-300 dark:bg-gray-600'
                }`}
              >
                <View
                  className={`w-5 h-5 rounded-full bg-white ${
                    pushEnabled ? 'ml-5' : 'mr-5'
                  }`}
                />
              </View>
            </Pressable>
          </View>
        );

      default:
        return null;
    }
  };

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="pt-12 px-6">
        <StepIndicator totalSteps={TOTAL_STEPS} currentStep={currentStep} />
      </View>

      {/* Content */}
      <ScrollView
        className="flex-1"
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 120 }}
      >
        {renderStep()}
      </ScrollView>

      {/* Bottom Buttons */}
      <View className="px-6 py-4 bg-background-light dark:bg-background-dark border-t border-gray-200 dark:border-gray-700">
        <View className="flex-row gap-3">
          {currentStep > 0 && (
            <Pressable
              onPress={handleBack}
              className="flex-1 border border-accent rounded-xl py-3 items-center"
              accessibilityRole="button"
              accessibilityLabel="Geri"
            >
              <Text className="text-accent font-semibold text-base">Geri</Text>
            </Pressable>
          )}
          <Pressable
            onPress={handleNext}
            disabled={!canProceed() || isSubmitting}
            className={`flex-1 rounded-xl py-3 items-center ${
              canProceed() && !isSubmitting
                ? 'bg-accent'
                : 'bg-gray-300 dark:bg-gray-600'
            }`}
            accessibilityRole="button"
            accessibilityLabel={
              currentStep === TOTAL_STEPS - 1 ? 'Hazır' : 'Növbəti'
            }
          >
            <Text className="text-white font-semibold text-base">
              {isSubmitting
                ? 'Yüklənir...'
                : currentStep === TOTAL_STEPS - 1
                  ? 'Hazır'
                  : 'Növbəti'}
            </Text>
          </Pressable>
        </View>
      </View>
    </View>
  );
}
