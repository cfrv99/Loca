import { View, Text, FlatList, Pressable, RefreshControl } from 'react-native';
import { useVenueGames } from '../hooks/use-venue-games';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';
import { EmptyState } from '../../../shared/components/empty-state';
import type { GameSessionDto } from '../../../shared/types';

interface Props {
  venueId: string;
}

const GAME_LABELS: Record<string, { name: string; icon: string }> = {
  mafia: { name: 'Mafia', icon: '🕵️' },
  truth_or_dare: { name: 'Həqiqət ya Cəza', icon: '🎯' },
  uno: { name: 'Uno', icon: '🃏' },
  domino: { name: 'Domino', icon: '🁡' },
  quiz: { name: 'Quiz', icon: '🧠' },
  would_you_rather: { name: 'Nəyi Seçərdin', icon: '🤔' },
};

function GameCard({ game }: { game: GameSessionDto }) {
  const info = GAME_LABELS[game.gameType] ?? {
    name: game.gameType,
    icon: '🎮',
  };
  const playerCount = game.players?.length ?? 0;

  return (
    <Pressable
      className="bg-white dark:bg-gray-800 rounded-2xl p-4 mx-4 mb-3"
      accessibilityRole="button"
      accessibilityLabel={`${info.name} oyunu, ${playerCount} oyuncu`}
    >
      <View className="flex-row items-center">
        <Text className="text-3xl mr-3">{info.icon}</Text>
        <View className="flex-1">
          <Text className="text-base font-semibold text-primary dark:text-white">
            {info.name}
          </Text>
          <Text className="text-xs text-gray-500 dark:text-gray-400">
            {playerCount}/{game.maxPlayers} oyuncu
          </Text>
        </View>
        <View
          className={`rounded-full px-3 py-1 ${
            game.status === 'lobby'
              ? 'bg-success/20'
              : game.status === 'in_progress'
                ? 'bg-warning/20'
                : 'bg-gray-200 dark:bg-gray-700'
          }`}
        >
          <Text
            className={`text-xs font-medium ${
              game.status === 'lobby'
                ? 'text-success'
                : game.status === 'in_progress'
                  ? 'text-warning'
                  : 'text-gray-500'
            }`}
          >
            {game.status === 'lobby'
              ? 'Gözlənilir'
              : game.status === 'in_progress'
                ? 'Davam edir'
                : 'Bitdi'}
          </Text>
        </View>
      </View>
    </Pressable>
  );
}

export function GamesList({ venueId }: Props) {
  const { data: games, isLoading, error, refetch, isRefetching } = useVenueGames(venueId);

  if (isLoading) return <LoadingSkeleton variant="default" />;
  if (error) return <ErrorState message="Oyunlar yuklenmedi" onRetry={refetch} />;
  if (!games || games.length === 0) {
    return <EmptyState icon="🎮" message="Hələ aktiv oyun yoxdur" />;
  }

  return (
    <View className="flex-1">
      {/* Create Game Button */}
      <Pressable
        className="bg-accent rounded-2xl py-3 mx-4 mt-2 mb-4 items-center"
        accessibilityRole="button"
        accessibilityLabel="Oyun yarat"
      >
        <Text className="text-white font-semibold text-base">Oyun yarat</Text>
      </Pressable>

      <FlatList
        data={games}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => <GameCard game={item} />}
        refreshControl={
          <RefreshControl refreshing={isRefetching} onRefresh={refetch} />
        }
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 100 }}
      />
    </View>
  );
}
