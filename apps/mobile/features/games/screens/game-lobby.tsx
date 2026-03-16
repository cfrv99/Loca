import { View, Text, Pressable, FlatList } from 'react-native';
import { useState, useEffect } from 'react';
import { useGameLobby } from '../hooks/use-game-lobby';
import { ConnectionStatus } from '../../chat/components/connection-status';
import { useAuthStore } from '../../../shared/stores/auth-store';
import type { GamePlayerDto } from '../../../shared/types';

interface Props {
  sessionId: string;
  onGameStarted?: () => void;
  onLeave?: () => void;
}

const GAME_TYPE_NAMES: Record<string, string> = {
  mafia: 'Mafiya',
  truth_or_dare: 'Həqiqət / Cəsarət',
  uno: 'Uno',
  domino: 'Domino',
  quiz: 'Quiz',
  would_you_rather: 'Nəyi Secirsən?',
};

export function GameLobbyScreen({ sessionId, onGameStarted, onLeave }: Props) {
  const {
    session,
    gameStarted,
    isConnected,
    joinGame,
    leaveGame,
    startGame,
    error,
  } = useGameLobby(sessionId);
  const { user } = useAuthStore();
  const [countdown, setCountdown] = useState<number | null>(null);

  // Join game on mount
  useEffect(() => {
    joinGame(sessionId);
    return () => {
      leaveGame(sessionId);
    };
  }, [sessionId, joinGame, leaveGame]);

  // Notify parent when game starts
  useEffect(() => {
    if (gameStarted) {
      onGameStarted?.();
    }
  }, [gameStarted, onGameStarted]);

  // Countdown effect
  useEffect(() => {
    if (countdown === null) return;
    if (countdown <= 0) return;
    const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
    return () => clearTimeout(timer);
  }, [countdown]);

  const isHost = session?.hostId === user?.id;
  const playerCount = session?.players.length ?? 0;
  const canStart = isHost && playerCount >= (session?.minPlayers ?? 2);
  const gameTypeName = GAME_TYPE_NAMES[session?.gameType ?? ''] ?? session?.gameType ?? '';

  const handleStart = async () => {
    setCountdown(3);
    setTimeout(() => {
      startGame(sessionId);
    }, 3000);
  };

  const handleLeave = () => {
    leaveGame(sessionId);
    onLeave?.();
  };

  const renderPlayer = ({ item }: { item: GamePlayerDto }) => (
    <View
      className="items-center mx-3 mb-4"
      accessibilityLabel={`${item.displayName} oyuncu`}
    >
      <View className="w-16 h-16 rounded-full bg-gray-200 dark:bg-gray-600 items-center justify-center mb-1">
        <Text className="text-2xl">
          {item.displayName.charAt(0).toUpperCase()}
        </Text>
      </View>
      <Text className="text-xs text-primary dark:text-white font-medium text-center" numberOfLines={1}>
        {item.displayName}
      </Text>
      {item.userId === session?.hostId && (
        <Text className="text-xs text-warning font-semibold">Host</Text>
      )}
      {item.isConnected ? (
        <View className="w-2 h-2 rounded-full bg-success mt-0.5" />
      ) : (
        <View className="w-2 h-2 rounded-full bg-gray-400 mt-0.5" />
      )}
    </View>
  );

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      <ConnectionStatus isConnected={isConnected} />

      {/* Header */}
      <View className="px-4 pt-6 pb-4 items-center">
        <Text className="text-2xl font-bold text-primary dark:text-white">
          {gameTypeName}
        </Text>
        <Text className="text-sm text-gray-500 dark:text-gray-400 mt-1">
          {playerCount} / {session?.maxPlayers ?? '?'} oyuncu
        </Text>
        <Text className="text-xs text-gray-400 dark:text-gray-500 mt-0.5">
          Minimum: {session?.minPlayers ?? '?'} oyuncu
        </Text>
      </View>

      {/* Countdown */}
      {countdown !== null && countdown > 0 && (
        <View className="items-center py-4">
          <Text className="text-6xl font-bold text-accent">{countdown}</Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Oyun baslayir...
          </Text>
        </View>
      )}

      {/* Players grid */}
      <View className="flex-1 px-4">
        <FlatList
          data={session?.players ?? []}
          keyExtractor={(item) => item.userId}
          renderItem={renderPlayer}
          numColumns={4}
          columnWrapperStyle={{ justifyContent: 'center' }}
          contentContainerStyle={{ paddingTop: 16 }}
          ListEmptyComponent={
            <View className="items-center py-8">
              <Text className="text-gray-400 dark:text-gray-500">
                Gozlənilir...
              </Text>
            </View>
          }
        />
      </View>

      {/* Error */}
      {error && (
        <View className="px-4 py-2">
          <Text className="text-sm text-error text-center">{error}</Text>
        </View>
      )}

      {/* Actions */}
      <View className="flex-row px-4 pb-6 gap-3">
        <Pressable
          onPress={handleLeave}
          className="flex-1 border border-error rounded-xl py-3 items-center"
          accessibilityLabel="Cix"
          accessibilityRole="button"
        >
          <Text className="text-error font-semibold">Cix</Text>
        </Pressable>

        {isHost && (
          <Pressable
            onPress={handleStart}
            disabled={!canStart || countdown !== null}
            className={`flex-1 rounded-xl py-3 items-center ${
              canStart && countdown === null ? 'bg-accent' : 'bg-gray-300 dark:bg-gray-600'
            }`}
            accessibilityLabel="Basla"
            accessibilityRole="button"
          >
            <Text className="text-white font-semibold">Basla</Text>
          </Pressable>
        )}
      </View>
    </View>
  );
}
