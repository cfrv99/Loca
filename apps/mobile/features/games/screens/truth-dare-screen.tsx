import { View, Text, Pressable, Animated } from 'react-native';
import { useState, useEffect, useRef } from 'react';
import { useSignalR } from '../../../shared/hooks/use-signalr';
import { CONFIG } from '../../../shared/constants/config';
import { ConnectionStatus } from '../../chat/components/connection-status';
import { useAuthStore } from '../../../shared/stores/auth-store';
import type { TruthDareState, GameResultDto } from '../../../shared/types';

interface Props {
  sessionId: string;
  onGameEnd?: () => void;
}

const CATEGORY_COLORS: Record<string, string> = {
  funny: 'bg-warning',
  romantic: 'bg-error',
  extreme: 'bg-secondary',
  intellectual: 'bg-accent',
};

export function TruthDareScreen({ sessionId, onGameEnd }: Props) {
  const { connection, isConnected } = useSignalR(CONFIG.SIGNALR_GAME_HUB);
  const { user } = useAuthStore();
  const [state, setState] = useState<TruthDareState | null>(null);
  const [result, setResult] = useState<GameResultDto | null>(null);
  const [timeRemaining, setTimeRemaining] = useState(0);
  const spinAnim = useRef(new Animated.Value(0)).current;
  const cardAnim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    if (!connection || !isConnected) return;

    const onStateUpdated = (sid: string, newState: TruthDareState) => {
      if (sid !== sessionId) return;
      setState(newState);
      setTimeRemaining(newState.timeoutSeconds);
    };

    const onGameEnded = (sid: string, gameResult: GameResultDto) => {
      if (sid !== sessionId) return;
      setResult(gameResult);
    };

    connection.on('stateUpdated', onStateUpdated);
    connection.on('gameEnded', onGameEnded);

    return () => {
      connection.off('stateUpdated', onStateUpdated);
      connection.off('gameEnded', onGameEnded);
    };
  }, [connection, isConnected, sessionId]);

  // Countdown timer
  useEffect(() => {
    if (timeRemaining <= 0) return;
    const timer = setInterval(() => {
      setTimeRemaining((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => clearInterval(timer);
  }, [timeRemaining]);

  // Spin animation
  useEffect(() => {
    if (state?.phase === 'spinning') {
      spinAnim.setValue(0);
      Animated.timing(spinAnim, {
        toValue: 1,
        duration: 2000,
        useNativeDriver: true,
      }).start();
    }
  }, [state?.phase, spinAnim]);

  // Card flip animation
  useEffect(() => {
    if (state?.phase === 'showing') {
      cardAnim.setValue(0);
      Animated.spring(cardAnim, {
        toValue: 1,
        friction: 6,
        tension: 80,
        useNativeDriver: true,
      }).start();
    }
  }, [state?.phase, cardAnim]);

  const handleChoice = async (choice: 'truth' | 'dare') => {
    if (!connection || !isConnected) return;
    await connection.invoke('SubmitAction', sessionId, {
      actionType: 'choose',
      data: { choice },
    });
  };

  const handleSkip = async () => {
    if (!connection || !isConnected) return;
    await connection.invoke('SubmitAction', sessionId, {
      actionType: 'skip',
    });
  };

  // ── GAME OVER ──
  if (result) {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-8">
        <Text className="text-4xl mb-4">🎉</Text>
        <Text className="text-2xl font-bold text-primary dark:text-white text-center mb-4">
          Oyun Bitdi!
        </Text>
        <Pressable
          onPress={onGameEnd}
          className="bg-accent rounded-xl py-3 px-8"
          accessibilityLabel="Lobiyə qayit"
          accessibilityRole="button"
        >
          <Text className="text-white font-semibold">Lobiyə qayit</Text>
        </Pressable>
      </View>
    );
  }

  if (!state) {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center">
        <Text className="text-lg text-gray-500 dark:text-gray-400">Gozlənilir...</Text>
      </View>
    );
  }

  const isMyTurn = state.currentPlayerId === user?.id;

  // ── SPINNING PHASE ──
  if (state.phase === 'spinning') {
    const spinRotation = spinAnim.interpolate({
      inputRange: [0, 1],
      outputRange: ['0deg', '1080deg'],
    });

    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center">
        <ConnectionStatus isConnected={isConnected} />
        <Animated.View
          style={{ transform: [{ rotate: spinRotation }] }}
          className="w-32 h-32 items-center justify-center"
        >
          <Text className="text-6xl">👆</Text>
        </Animated.View>
        <Text className="text-lg text-gray-500 dark:text-gray-400 mt-4">
          Kim seciləcək...
        </Text>
      </View>
    );
  }

  // ── CHOOSING PHASE (Truth or Dare) ──
  if (state.phase === 'choosing') {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-8">
        <ConnectionStatus isConnected={isConnected} />
        <Text className="text-lg text-gray-500 dark:text-gray-400 mb-2">
          Novbə:
        </Text>
        <Text className="text-2xl font-bold text-primary dark:text-white mb-8">
          {state.currentPlayerName}
        </Text>

        {isMyTurn ? (
          <View className="w-full gap-4">
            <Pressable
              onPress={() => handleChoice('truth')}
              className="bg-accent rounded-2xl py-6 items-center"
              accessibilityLabel="Həqiqət sec"
              accessibilityRole="button"
            >
              <Text className="text-3xl mb-2">🤔</Text>
              <Text className="text-xl font-bold text-white">Həqiqət</Text>
            </Pressable>
            <Pressable
              onPress={() => handleChoice('dare')}
              className="bg-error rounded-2xl py-6 items-center"
              accessibilityLabel="Cəsarət sec"
              accessibilityRole="button"
            >
              <Text className="text-3xl mb-2">💪</Text>
              <Text className="text-xl font-bold text-white">Cəsarət</Text>
            </Pressable>
          </View>
        ) : (
          <Text className="text-base text-gray-500 dark:text-gray-400 text-center">
            {state.currentPlayerName} secir...
          </Text>
        )}
      </View>
    );
  }

  // ── SHOWING PHASE (Question/Challenge displayed) ──
  if (state.phase === 'showing') {
    const categoryColor = CATEGORY_COLORS[state.questionCategory ?? ''] ?? 'bg-gray-500';
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-6">
        <ConnectionStatus isConnected={isConnected} />

        {/* Timer */}
        <Text className="text-3xl font-bold text-warning mb-4">
          {timeRemaining}s
        </Text>

        {/* Turn indicator */}
        <Text className="text-sm text-gray-500 dark:text-gray-400 mb-2">
          {state.currentPlayerName} — {state.questionType === 'truth' ? 'Həqiqət' : 'Cəsarət'}
        </Text>

        {/* Question card */}
        <Animated.View
          style={{
            transform: [
              { scale: cardAnim.interpolate({ inputRange: [0, 1], outputRange: [0.5, 1] }) },
            ],
            opacity: cardAnim,
          }}
          className="bg-white dark:bg-gray-800 rounded-3xl p-8 shadow-lg w-full"
        >
          {/* Category badge */}
          {state.questionCategory && (
            <View className={`self-start rounded-full px-3 py-1 mb-4 ${categoryColor}`}>
              <Text className="text-xs text-white font-semibold">
                {state.questionCategory}
              </Text>
            </View>
          )}

          <Text className="text-xl font-semibold text-primary dark:text-white text-center leading-8">
            {state.questionText ?? '...'}
          </Text>
        </Animated.View>

        {/* Skip button (costs 50 coins) */}
        {isMyTurn && (
          <Pressable
            onPress={handleSkip}
            className="mt-6 border border-warning rounded-xl py-2 px-6"
            accessibilityLabel="Kec, 50 coin"
            accessibilityRole="button"
          >
            <Text className="text-warning font-medium">
              Kec (50 coin)
            </Text>
          </Pressable>
        )}
      </View>
    );
  }

  // ── WAITING ──
  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center">
      <ConnectionStatus isConnected={isConnected} />
      <Text className="text-4xl mb-4">⏳</Text>
      <Text className="text-lg text-gray-500 dark:text-gray-400">
        Gozlənilir...
      </Text>
    </View>
  );
}
