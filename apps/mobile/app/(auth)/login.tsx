import { View, Text, Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '@/shared/stores/auth-store';
import { api } from '@/shared/services/api-client';
import type { ApiResponse, LoginResult } from '@/shared/types';

export default function LoginScreen() {
  const router = useRouter();
  const { setUser, setTokens } = useAuthStore();

  const handleGoogleLogin = async () => {
    try {
      // In production, use expo-auth-session for Google OAuth
      // For dev, we use the simplified token format
      const res = await api.post<ApiResponse<LoginResult>>('/auth/google', {
        idToken: 'google:dev@loca.az:google-dev-123:Dev User',
        deviceInfo: 'Expo Dev',
      });

      if (res.data.success && res.data.data) {
        const { accessToken, refreshToken, user } = res.data.data;
        await setTokens(accessToken, refreshToken);
        setUser({
          id: user.id,
          email: user.email,
          displayName: user.displayName,
          profilePhotoUrl: user.profilePhotoUrl,
          isOnboardingComplete: user.isOnboardingComplete,
        });

        if (user.isOnboardingComplete) {
          router.replace('/(tabs)/discover');
        } else {
          router.replace('/(auth)/onboarding');
        }
      }
    } catch (error) {
      if (__DEV__) console.error('Login failed:', error);
    }
  };

  return (
    <View className="flex-1 bg-primary items-center justify-center px-8">
      {/* Logo */}
      <View className="mb-12 items-center">
        <Text className="text-5xl font-bold text-white mb-2">LOCA</Text>
        <Text className="text-lg text-gray-300">
          Discover. Connect. Play.
        </Text>
      </View>

      {/* Login Buttons */}
      <View className="w-full gap-4">
        <Pressable
          onPress={handleGoogleLogin}
          className="bg-white rounded-xl py-4 px-6 flex-row items-center justify-center active:opacity-80"
          accessibilityRole="button"
          accessibilityLabel="Google ile daxil ol"
        >
          <Text className="text-primary font-semibold text-base ml-3">
            Google ile daxil ol
          </Text>
        </Pressable>

        <Pressable
          className="bg-black rounded-xl py-4 px-6 flex-row items-center justify-center active:opacity-80"
          accessibilityRole="button"
          accessibilityLabel="Apple ile daxil ol"
        >
          <Text className="text-white font-semibold text-base ml-3">
            Apple ile daxil ol
          </Text>
        </Pressable>
      </View>

      {/* Terms */}
      <Text className="text-xs text-gray-400 text-center mt-8 px-4">
        Daxil olmaqla Istifade Sertleri ve Mexfilik Siyasetimizi qebul
        edirsiniz.
      </Text>
    </View>
  );
}
