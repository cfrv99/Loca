import { View, Text, Pressable, FlatList } from 'react-native';
import { useQuery } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import type { ApiResponse, GiftDto } from '../../../shared/types';

interface Props {
  onSelect: (gift: GiftDto) => void;
  onClose: () => void;
  coinBalance: number;
}

const TIER_ORDER = ['basic', 'premium', 'luxury'];

export function GiftPicker({ onSelect, onClose, coinBalance }: Props) {
  const { data: gifts, isLoading } = useQuery({
    queryKey: ['gifts', 'catalog'],
    queryFn: async () => {
      const res = await api.get<ApiResponse<GiftDto[]>>('/economy/gifts');
      if (!res.data.success || !res.data.data) throw new Error('Gift kataloqu yuklenmədi');
      return res.data.data;
    },
    staleTime: 1000 * 60 * 10,
  });

  if (isLoading) return <LoadingSkeleton variant="default" />;

  const sortedGifts = [...(gifts ?? [])].sort((a, b) => {
    const aIdx = TIER_ORDER.indexOf(a.tier);
    const bIdx = TIER_ORDER.indexOf(b.tier);
    if (aIdx !== bIdx) return aIdx - bIdx;
    return a.coinPrice - b.coinPrice;
  });

  return (
    <View className="bg-white dark:bg-gray-800 rounded-t-3xl p-4 max-h-96">
      {/* Header */}
      <View className="flex-row items-center justify-between mb-4">
        <Text className="text-lg font-semibold text-primary dark:text-white">
          Hədiyyə sec
        </Text>
        <View className="flex-row items-center">
          <Text className="text-sm text-accent font-medium mr-3">
            {coinBalance} coin
          </Text>
          <Pressable
            onPress={onClose}
            accessibilityLabel="Baglat"
            accessibilityRole="button"
          >
            <Text className="text-gray-500 text-lg">✕</Text>
          </Pressable>
        </View>
      </View>

      {/* Gift grid */}
      <FlatList
        data={sortedGifts}
        keyExtractor={(item) => item.id}
        numColumns={4}
        renderItem={({ item }) => {
          const canAfford = coinBalance >= item.coinPrice;
          return (
            <Pressable
              onPress={() => canAfford && onSelect(item)}
              disabled={!canAfford}
              className={`flex-1 items-center p-2 m-1 rounded-xl ${
                canAfford
                  ? 'bg-gray-50 dark:bg-gray-700 active:bg-gray-100'
                  : 'bg-gray-100 dark:bg-gray-600 opacity-50'
              }`}
              accessibilityLabel={`${item.nameAz ?? item.name}, ${item.coinPrice} coin`}
              accessibilityRole="button"
            >
              <Text className="text-2xl mb-1">
                {item.iconUrl ?? '🎁'}
              </Text>
              <Text className="text-xs text-primary dark:text-white font-medium text-center" numberOfLines={1}>
                {item.nameAz ?? item.name}
              </Text>
              <Text className="text-xs text-accent">{item.coinPrice}</Text>
            </Pressable>
          );
        }}
        showsVerticalScrollIndicator={false}
      />
    </View>
  );
}
