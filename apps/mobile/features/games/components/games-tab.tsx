import { View, Text, Pressable, FlatList, Modal } from 'react-native';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { EmptyState } from '../../../shared/components/empty-state';
import type { ApiResponse, GameSessionDto } from '../../../shared/types';

interface Props {
  venueId: string;
  onJoinGame: (sessionId: string) => void;
  onCreateGame: (gameType: string, maxPlayers: number) => void;
}

const GAME_TYPES = [
  { type: 'mafia', name: 'Mafiya', icon: '🔪', min: 5, max: 12 },
  { type: 'truth_or_dare', name: 'Həqiqət / Cəsarət', icon: '🤔', min: 3, max: 10 },
  { type: 'uno', name: 'Uno', icon: '🃏', min: 2, max: 6 },
  { type: 'domino', name: 'Domino', icon: '🀄', min: 2, max: 4 },
  { type: 'quiz', name: 'Quiz', icon: '🧠', min: 2, max: 20 },
  { type: 'would_you_rather', name: 'Nəyi Secirsən?', icon: '🤷', min: 2, max: 20 },
];

const GAME_TYPE_NAMES: Record<string, string> = Object.fromEntries(
  GAME_TYPES.map((g) => [g.type, g.name]),
);

const GAME_TYPE_ICONS: Record<string, string> = Object.fromEntries(
  GAME_TYPES.map((g) => [g.type, g.icon]),
);

export function GamesTab({ venueId, onJoinGame, onCreateGame }: Props) {
  const [showCreateModal, setShowCreateModal] = useState(false);

  const { data: sessions, isLoading } = useQuery({
    queryKey: ['games', venueId, 'active'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<GameSessionDto[]>>(
        `/venues/${venueId}/games`,
      );
      if (!res.data.success) return [];
      return res.data.data ?? [];
    },
    staleTime: 1000 * 10,
    refetchInterval: 1000 * 15,
  });

  if (isLoading) return <LoadingSkeleton variant="default" />;

  const activeGames = sessions ?? [];

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="px-4 py-3 flex-row items-center justify-between">
        <Text className="text-lg font-semibold text-primary dark:text-white">
          Aktiv Oyunlar
        </Text>
        <Text className="text-sm text-gray-400">
          {activeGames.length} oyun
        </Text>
      </View>

      {activeGames.length === 0 ? (
        <EmptyState
          icon="🎮"
          message="Hələ aktiv oyun yoxdur. İlk oyunu sən yarat!"
        />
      ) : (
        <FlatList
          data={activeGames}
          keyExtractor={(item) => item.id}
          contentContainerStyle={{ paddingHorizontal: 16, paddingBottom: 80 }}
          renderItem={({ item }) => (
            <Pressable
              onPress={() => onJoinGame(item.id)}
              className="bg-white dark:bg-gray-800 rounded-2xl p-4 mb-3 flex-row items-center"
              accessibilityLabel={`${GAME_TYPE_NAMES[item.gameType] ?? item.gameType} oyunu, ${item.players.length} oyuncu`}
              accessibilityRole="button"
            >
              <Text className="text-3xl mr-3">
                {GAME_TYPE_ICONS[item.gameType] ?? '🎮'}
              </Text>
              <View className="flex-1">
                <Text className="text-base font-semibold text-primary dark:text-white">
                  {GAME_TYPE_NAMES[item.gameType] ?? item.gameType}
                </Text>
                <Text className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  {item.players.length}/{item.maxPlayers} oyuncu
                </Text>
              </View>
              <View className={`px-3 py-1 rounded-full ${
                item.status === 'lobby' ? 'bg-success/20' : 'bg-warning/20'
              }`}>
                <Text className={`text-xs font-semibold ${
                  item.status === 'lobby' ? 'text-success' : 'text-warning'
                }`}>
                  {item.status === 'lobby' ? 'Qosul' : 'Davam edir'}
                </Text>
              </View>
            </Pressable>
          )}
        />
      )}

      {/* Create game FAB */}
      <Pressable
        onPress={() => setShowCreateModal(true)}
        className="absolute bottom-6 right-6 bg-accent rounded-full w-14 h-14 items-center justify-center shadow-lg"
        accessibilityLabel="Oyun yarat"
        accessibilityRole="button"
      >
        <Text className="text-white text-2xl font-bold">+</Text>
      </Pressable>

      {/* Create game modal */}
      <Modal
        visible={showCreateModal}
        animationType="slide"
        transparent
        onRequestClose={() => setShowCreateModal(false)}
      >
        <View className="flex-1 bg-black/50 justify-end">
          <View className="bg-white dark:bg-gray-800 rounded-t-3xl p-4">
            <View className="flex-row items-center justify-between mb-4">
              <Text className="text-lg font-semibold text-primary dark:text-white">
                Oyun yarat
              </Text>
              <Pressable
                onPress={() => setShowCreateModal(false)}
                accessibilityLabel="Baglat"
                accessibilityRole="button"
              >
                <Text className="text-gray-500 text-lg">✕</Text>
              </Pressable>
            </View>

            {GAME_TYPES.map((game) => (
              <Pressable
                key={game.type}
                onPress={() => {
                  setShowCreateModal(false);
                  onCreateGame(game.type, game.max);
                }}
                className="flex-row items-center p-4 mb-2 bg-gray-50 dark:bg-gray-700 rounded-xl active:bg-gray-100"
                accessibilityLabel={`${game.name} yarat`}
                accessibilityRole="button"
              >
                <Text className="text-2xl mr-3">{game.icon}</Text>
                <View className="flex-1">
                  <Text className="text-base font-semibold text-primary dark:text-white">
                    {game.name}
                  </Text>
                  <Text className="text-xs text-gray-500 dark:text-gray-400">
                    {game.min}-{game.max} oyuncu
                  </Text>
                </View>
              </Pressable>
            ))}
          </View>
        </View>
      </Modal>
    </View>
  );
}
