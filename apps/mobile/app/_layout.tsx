import { Stack } from 'expo-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { StatusBar } from 'expo-status-bar';
import { GiftAnimationProvider } from '../features/gifting/context/gift-animation-context';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 2,
      retry: 2,
    },
  },
});

export default function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <GiftAnimationProvider>
        <StatusBar style="auto" />
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="(auth)/login" />
          <Stack.Screen name="(auth)/onboarding" />
          <Stack.Screen name="(tabs)" />
          <Stack.Screen name="venue/[id]" />
          <Stack.Screen name="venue/[id]/scan" options={{ presentation: 'fullScreenModal' }} />
          <Stack.Screen name="user/[id]" />
          <Stack.Screen name="coin-shop" />
          <Stack.Screen name="conversation/[id]" />
          <Stack.Screen name="game/[id]" />
        </Stack>
      </GiftAnimationProvider>
    </QueryClientProvider>
  );
}
