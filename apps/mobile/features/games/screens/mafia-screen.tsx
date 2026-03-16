import { View, Text, Pressable, FlatList, Animated } from 'react-native';
import { useState, useEffect, useRef } from 'react';
import { useMafiaGame } from '../hooks/use-mafia-game';
import { ConnectionStatus } from '../../chat/components/connection-status';
import { useAuthStore } from '../../../shared/stores/auth-store';

interface Props {
  sessionId: string;
  onGameEnd?: () => void;
}

const ROLE_NAMES: Record<string, string> = {
  mafia: 'Mafiya',
  doctor: 'Doktor',
  detective: 'Dedektiv',
  citizen: 'Vətəndas',
};

const ROLE_ICONS: Record<string, string> = {
  mafia: '🔪',
  doctor: '💊',
  detective: '🔍',
  citizen: '👤',
};

const PHASE_NAMES: Record<string, string> = {
  night: 'Gecə',
  day_discussion: 'Gunduz Muzakirə',
  day_vote: 'Gunduz Səsvermə',
  elimination: 'Kənarlasdirma',
  role_reveal: 'Rol Asilmasi',
  game_over: 'Oyun Bitdi',
};

export function MafiaScreen({ sessionId, onGameEnd }: Props) {
  const {
    myRole,
    phase,
    result,
    eliminatedPlayer,
    timeRemaining,
    isConnected,
    submitVote,
    submitNightAction,
  } = useMafiaGame(sessionId);
  const { user } = useAuthStore();
  const [selectedTarget, setSelectedTarget] = useState<string | null>(null);
  const [hasActed, setHasActed] = useState(false);
  const roleRevealAnim = useRef(new Animated.Value(0)).current;

  // Role reveal animation
  useEffect(() => {
    if (myRole && phase?.phase === 'role_reveal') {
      Animated.spring(roleRevealAnim, {
        toValue: 1,
        friction: 6,
        tension: 80,
        useNativeDriver: true,
      }).start();
    }
  }, [myRole, phase?.phase, roleRevealAnim]);

  // Reset action state on phase change
  useEffect(() => {
    setSelectedTarget(null);
    setHasActed(false);
  }, [phase?.phase]);

  const handleTargetSelect = (playerId: string) => {
    if (hasActed) return;
    setSelectedTarget(playerId);
  };

  const handleConfirmAction = async () => {
    if (!selectedTarget || hasActed) return;
    setHasActed(true);
    if (phase?.phase === 'night') {
      await submitNightAction(selectedTarget);
    } else if (phase?.phase === 'day_vote') {
      await submitVote(selectedTarget);
    }
  };

  // ── GAME OVER ──
  if (result) {
    const winnerText = result.winnerTeam === 'mafia'
      ? 'Mafiya qalib oldu!'
      : 'Vətəndaslar qalib oldu!';
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-8">
        <Text className="text-4xl mb-4">
          {result.winnerTeam === 'mafia' ? '🔪' : '🏆'}
        </Text>
        <Text className="text-2xl font-bold text-primary dark:text-white text-center mb-2">
          {winnerText}
        </Text>
        <Text className="text-sm text-gray-500 dark:text-gray-400 mb-6">
          Muddet: {result.duration}
        </Text>
        {result.scores.map((s) => (
          <View key={s.userId} className="flex-row items-center mb-2">
            <Text className="text-base text-primary dark:text-white flex-1">
              {s.displayName}
            </Text>
            <Text className="text-base font-semibold text-accent">
              {s.score} xal
            </Text>
          </View>
        ))}
        <Pressable
          onPress={onGameEnd}
          className="bg-accent rounded-xl py-3 px-8 mt-6"
          accessibilityLabel="Lobiyə qayit"
          accessibilityRole="button"
        >
          <Text className="text-white font-semibold">Lobiyə qayit</Text>
        </Pressable>
      </View>
    );
  }

  // ── ROLE REVEAL ──
  if (myRole && phase?.phase === 'role_reveal') {
    const roleName = ROLE_NAMES[myRole.role] ?? myRole.role;
    const roleIcon = ROLE_ICONS[myRole.role] ?? '?';
    return (
      <View className="flex-1 bg-primary items-center justify-center px-8">
        <Animated.View
          style={{
            transform: [
              { scale: roleRevealAnim.interpolate({ inputRange: [0, 1], outputRange: [0.3, 1] }) },
              { rotateY: roleRevealAnim.interpolate({ inputRange: [0, 1], outputRange: ['90deg', '0deg'] }) },
            ],
            opacity: roleRevealAnim,
          }}
          className="bg-white dark:bg-gray-800 rounded-3xl p-8 items-center shadow-lg"
        >
          <Text className="text-6xl mb-4">{roleIcon}</Text>
          <Text className="text-2xl font-bold text-primary dark:text-white mb-2">
            {roleName}
          </Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400 text-center">
            {myRole.role === 'mafia' && 'Gecələr birgə qurban secin'}
            {myRole.role === 'doctor' && 'Gecələr birini xilas edin'}
            {myRole.role === 'detective' && 'Gecələr birini arasdirin'}
            {myRole.role === 'citizen' && 'Gunduz mafiyani tapin və səs verin'}
          </Text>
          {myRole.mafiaTeamIds && myRole.mafiaTeamIds.length > 0 && (
            <Text className="text-xs text-error mt-2">
              Mafiya ortaqlarin: {myRole.mafiaTeamIds.length} nəfər
            </Text>
          )}
        </Animated.View>
      </View>
    );
  }

  // ── ELIMINATION ──
  if (eliminatedPlayer) {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-8">
        <Text className="text-4xl mb-4">💀</Text>
        <Text className="text-xl font-bold text-primary dark:text-white text-center mb-2">
          {eliminatedPlayer.displayName} kənarlasdırildi!
        </Text>
        {eliminatedPlayer.role && (
          <Text className="text-base text-gray-500 dark:text-gray-400">
            Rolu: {ROLE_NAMES[eliminatedPlayer.role] ?? eliminatedPlayer.role}
          </Text>
        )}
      </View>
    );
  }

  // ── NIGHT PHASE ──
  if (phase?.phase === 'night') {
    const isMyTurn = myRole?.role !== 'citizen';
    const actionLabel = myRole?.role === 'mafia' ? 'Qurban sec'
      : myRole?.role === 'doctor' ? 'Xilas et'
      : myRole?.role === 'detective' ? 'Arasdır'
      : '';

    if (!isMyTurn) {
      return (
        <View className="flex-1 bg-primary items-center justify-center px-8">
          <ConnectionStatus isConnected={isConnected} />
          <Text className="text-4xl mb-4">🌙</Text>
          <Text className="text-xl font-bold text-white text-center mb-2">
            Gecədir...
          </Text>
          <Text className="text-sm text-gray-300 text-center">
            Gozləyin, rollar hərəkət edir
          </Text>
          <Text className="text-3xl font-bold text-warning mt-4">
            {timeRemaining}s
          </Text>
        </View>
      );
    }

    return (
      <View className="flex-1 bg-primary">
        <ConnectionStatus isConnected={isConnected} />
        <View className="px-4 pt-6 pb-4 items-center">
          <Text className="text-lg font-bold text-white">
            🌙 {PHASE_NAMES.night} — {actionLabel}
          </Text>
          <Text className="text-3xl font-bold text-warning mt-2">
            {timeRemaining}s
          </Text>
        </View>
        <FlatList
          data={phase.alivePlayers.filter((p) => p.userId !== user?.id)}
          keyExtractor={(item) => item.userId}
          numColumns={3}
          columnWrapperStyle={{ justifyContent: 'center' }}
          contentContainerStyle={{ paddingHorizontal: 16 }}
          renderItem={({ item }) => (
            <Pressable
              onPress={() => handleTargetSelect(item.userId)}
              disabled={hasActed}
              className={`m-2 items-center p-3 rounded-2xl ${
                selectedTarget === item.userId
                  ? 'bg-error/30 border-2 border-error'
                  : 'bg-white/10'
              }`}
              accessibilityLabel={`${item.displayName} sec`}
              accessibilityRole="button"
            >
              <View className="w-14 h-14 rounded-full bg-gray-300/30 items-center justify-center mb-1">
                <Text className="text-xl">
                  {item.displayName.charAt(0).toUpperCase()}
                </Text>
              </View>
              <Text className="text-xs text-white text-center" numberOfLines={1}>
                {item.displayName}
              </Text>
            </Pressable>
          )}
        />
        {selectedTarget && !hasActed && (
          <View className="px-4 pb-6">
            <Pressable
              onPress={handleConfirmAction}
              className="bg-error rounded-xl py-3 items-center"
              accessibilityLabel="Təsdiq et"
              accessibilityRole="button"
            >
              <Text className="text-white font-semibold">Təsdiq et</Text>
            </Pressable>
          </View>
        )}
        {hasActed && (
          <View className="px-4 pb-6 items-center">
            <Text className="text-sm text-gray-300">
              Secim edildi. Digərlərini gozləyin...
            </Text>
          </View>
        )}
      </View>
    );
  }

  // ── DAY DISCUSSION ──
  if (phase?.phase === 'day_discussion') {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark">
        <ConnectionStatus isConnected={isConnected} />
        <View className="px-4 pt-6 pb-4 items-center">
          <Text className="text-lg font-bold text-primary dark:text-white">
            ☀️ {PHASE_NAMES.day_discussion}
          </Text>
          <Text className="text-3xl font-bold text-warning mt-2">
            {timeRemaining}s
          </Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Kimin mafiya oldugunu muzakirə edin
          </Text>
        </View>

        {/* Alive/dead indicators */}
        <View className="px-4">
          <Text className="text-sm font-semibold text-primary dark:text-white mb-2">
            Sag qalanlar:
          </Text>
          <FlatList
            data={phase.alivePlayers}
            keyExtractor={(item) => item.userId}
            horizontal
            showsHorizontalScrollIndicator={false}
            renderItem={({ item }) => (
              <View className="items-center mx-2">
                <View className="w-12 h-12 rounded-full bg-gray-200 dark:bg-gray-600 items-center justify-center">
                  <Text className="text-lg">
                    {item.displayName.charAt(0).toUpperCase()}
                  </Text>
                </View>
                <Text className="text-xs text-primary dark:text-white mt-1" numberOfLines={1}>
                  {item.displayName}
                </Text>
                {item.userId === user?.id && (
                  <Text className="text-xs text-accent">Sən</Text>
                )}
              </View>
            )}
          />
        </View>

        {/* Investigation result for detective */}
        {myRole?.role === 'detective' && myRole.investigationResult !== undefined && (
          <View className="mx-4 mt-4 p-3 bg-accent/10 rounded-xl">
            <Text className="text-sm text-accent font-medium">
              🔍 Arasdırma nəticəsi: {myRole.investigationResult ? 'Mafiya!' : 'Mafiya deyil'}
            </Text>
          </View>
        )}
      </View>
    );
  }

  // ── DAY VOTE ──
  if (phase?.phase === 'day_vote') {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark">
        <ConnectionStatus isConnected={isConnected} />
        <View className="px-4 pt-6 pb-4 items-center">
          <Text className="text-lg font-bold text-primary dark:text-white">
            🗳️ {PHASE_NAMES.day_vote}
          </Text>
          <Text className="text-3xl font-bold text-warning mt-2">
            {timeRemaining}s
          </Text>
          <Text className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Kim kənarlasdırılsın?
          </Text>
        </View>
        <FlatList
          data={phase.alivePlayers.filter((p) => p.userId !== user?.id)}
          keyExtractor={(item) => item.userId}
          contentContainerStyle={{ paddingHorizontal: 16 }}
          renderItem={({ item }) => {
            const voteCount = phase.votes
              ? Object.values(phase.votes).filter((v) => v === item.userId).length
              : 0;
            return (
              <Pressable
                onPress={() => handleTargetSelect(item.userId)}
                disabled={hasActed}
                className={`flex-row items-center p-4 mb-2 rounded-2xl ${
                  selectedTarget === item.userId
                    ? 'bg-error/10 border-2 border-error'
                    : 'bg-white dark:bg-gray-800'
                }`}
                accessibilityLabel={`${item.displayName} ucun səs ver`}
                accessibilityRole="button"
              >
                <View className="w-12 h-12 rounded-full bg-gray-200 dark:bg-gray-600 items-center justify-center mr-3">
                  <Text className="text-lg">
                    {item.displayName.charAt(0).toUpperCase()}
                  </Text>
                </View>
                <Text className="flex-1 text-base text-primary dark:text-white font-medium">
                  {item.displayName}
                </Text>
                {voteCount > 0 && (
                  <View className="bg-error/20 rounded-full px-2 py-0.5">
                    <Text className="text-xs text-error font-semibold">{voteCount} səs</Text>
                  </View>
                )}
              </Pressable>
            );
          }}
        />
        {selectedTarget && !hasActed && (
          <View className="px-4 pb-6">
            <Pressable
              onPress={handleConfirmAction}
              className="bg-error rounded-xl py-3 items-center"
              accessibilityLabel="Səs ver"
              accessibilityRole="button"
            >
              <Text className="text-white font-semibold">Səs ver</Text>
            </Pressable>
          </View>
        )}
        {hasActed && (
          <View className="px-4 pb-6 items-center">
            <Text className="text-sm text-gray-500 dark:text-gray-400">
              Səsiniz qeydə alindi. Digərlərini gozləyin...
            </Text>
          </View>
        )}
      </View>
    );
  }

  // ── DEFAULT / WAITING ──
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
