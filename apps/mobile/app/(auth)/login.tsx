import { View, Text, Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '../../shared/stores/auth-store';
import { api } from '../../shared/services/api-client';
import type { ApiResponse, AuthResponse } from '../../shared/types';

export default function LoginScreen() {
  const router = useRouter();
  const { setTokens, setUser } = useAuthStore();

  const handleGoogleLogin = async () => {
    try {
      // In production: use expo-auth-session for Google OAuth
      // For dev: mock login
      const res = await api.post<ApiResponse<AuthResponse>>('/auth/google', {
        idToken: 'dev-mock-token',
      });

      if (res.data.success && res.data.data) {
        const { accessToken, refreshToken, user, isNewUser } = res.data.data;
        setTokens(accessToken, refreshToken);
        setUser(user);

        // Navigate based on onboarding status
        if (isNewUser || !user.isOnboarded) {
          router.replace('/(auth)/onboarding');
        } else {
          router.replace('/(tabs)/discover');
        }
      }
    } catch (error) {
      if (__DEV__) console.error('Login failed:', error);
    }
  };

  return (
    <View className="flex-1 items-center justify-center bg-primary px-8">
      {/* Logo */}
      <Text className="text-6xl font-bold text-white mb-2">Loca</Text>
      <Text className="text-lg text-gray-300 mb-12 text-center">
        Məkanları kəşf et, insanlarla tanış ol
      </Text>

      {/* Google Login */}
      <Pressable
        onPress={handleGoogleLogin}
        className="w-full bg-white rounded-xl py-4 px-6 flex-row items-center justify-center mb-4"
        accessibilityRole="button"
        accessibilityLabel="Google ilə daxil ol"
      >
        <Text className="text-primary font-semibold text-base">
          Google ilə daxil ol
        </Text>
      </Pressable>

      {/* Apple Login */}
      <Pressable
        className="w-full bg-black rounded-xl py-4 px-6 flex-row items-center justify-center mb-8"
        accessibilityRole="button"
        accessibilityLabel="Apple ilə daxil ol"
      >
        <Text className="text-white font-semibold text-base">
          Apple ilə daxil ol
        </Text>
      </Pressable>

      {/* Age notice */}
      <Text className="text-xs text-gray-400 text-center">
        Daxil olmaqla, 18 yaşdan yuxarı olduğunuzu və{'\n'}İstifadə Şərtlərini qəbul etdiyinizi təsdiqləyirsiniz.
      </Text>
    </View>
  );
}
