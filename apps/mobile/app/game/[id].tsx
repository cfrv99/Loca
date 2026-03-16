import { View, Pressable, Text } from 'react-native';
import { useState } from 'react';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useQuery } from '@tanstack/react-query';
import { GameLobbyScreen } from '../../features/games/screens/game-lobby';
import { MafiaScreen } from '../../features/games/screens/mafia-screen';
import { TruthDareScreen } from '../../features/games/screens/truth-dare-screen';
import { LoadingSkeleton } from '../../shared/components/loading-skeleton';
import { api } from '../../shared/services/api-client';
import type { ApiResponse, GameSessionDto } from '../../shared/types';

type GamePhase = 'lobby' | 'playing';

export default function GameScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const [phase, setPhase] = useState<GamePhase>('lobby');

  // Get game session info to determine type
  const { data: session } = useQuery({
    queryKey: ['game', id],
    queryFn: async () => {
      const res = await api.get<ApiResponse<GameSessionDto>>(`/games/${id}`);
      if (!res.data.success || !res.data.data) return null;
      return res.data.data;
    },
    enabled: !!id,
    staleTime: 1000 * 30,
  });

  if (!id) {
    return <LoadingSkeleton variant="default" />;
  }

  const handleGameEnd = () => {
    router.back();
  };

  const handleGameStarted = () => {
    setPhase('playing');
  };

  const handleLeave = () => {
    router.back();
  };

  // Lobby phase
  if (phase === 'lobby') {
    return (
      <View className="flex-1">
        {/* Back button */}
        <View className="px-4 pt-12 pb-2 bg-background-light dark:bg-background-dark">
          <Pressable
            onPress={() => router.back()}
            className="py-2"
            accessibilityLabel="Geri"
            accessibilityRole="button"
          >
            <Text className="text-accent text-lg">{'<'} Geri</Text>
          </Pressable>
        </View>
        <GameLobbyScreen
          sessionId={id}
          onGameStarted={handleGameStarted}
          onLeave={handleLeave}
        />
      </View>
    );
  }

  // Playing phase - render game-specific screen
  const gameType = session?.gameType ?? 'mafia';

  if (gameType === 'mafia') {
    return <MafiaScreen sessionId={id} onGameEnd={handleGameEnd} />;
  }

  if (gameType === 'truth_or_dare') {
    return <TruthDareScreen sessionId={id} onGameEnd={handleGameEnd} />;
  }

  // Fallback for other game types
  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-8">
      <Text className="text-4xl mb-4">🎮</Text>
      <Text className="text-xl font-bold text-primary dark:text-white text-center mb-2">
        {gameType} oyunu
      </Text>
      <Text className="text-sm text-gray-500 dark:text-gray-400 text-center mb-6">
        Bu oyun novusu tezliklə əlavə olunacaq
      </Text>
      <Pressable
        onPress={handleGameEnd}
        className="bg-accent rounded-xl py-3 px-8"
        accessibilityLabel="Geri qayit"
        accessibilityRole="button"
      >
        <Text className="text-white font-semibold">Geri qayit</Text>
      </Pressable>
    </View>
  );
}
