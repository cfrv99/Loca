import { View, Text, Pressable, ScrollView, Alert } from 'react-native';
import { useRouter } from 'expo-router';
import { useBalance } from '../hooks/use-economy';
import { LoadingSkeleton } from '../../../shared/components/loading-skeleton';
import { ErrorState } from '../../../shared/components/error-state';

interface PackageDisplay {
  id: string;
  nameAz: string;
  priceAzn: number;
  coins: number;
  bonus: number;
  highlight?: boolean;
}

const PACKAGES: PackageDisplay[] = [
  {
    id: 'first_buy',
    nameAz: 'İlk Alış',
    priceAzn: 2.99,
    coins: 300,
    bonus: 50,
  },
  {
    id: 'standard',
    nameAz: 'Standard',
    priceAzn: 9.99,
    coins: 1100,
    bonus: 100,
  },
  {
    id: 'premium',
    nameAz: 'Premium',
    priceAzn: 24.99,
    coins: 3000,
    bonus: 500,
    highlight: true,
  },
  {
    id: 'vip',
    nameAz: 'VIP',
    priceAzn: 49.99,
    coins: 7000,
    bonus: 1500,
  },
];

export function CoinShopScreen() {
  const router = useRouter();
  const { data: balance, isLoading, error, refetch } = useBalance();

  const handlePurchase = (pkg: PackageDisplay) => {
    Alert.alert(
      'Tezliklə',
      `${pkg.nameAz} paketi tezliklə mövcud olacaq. ${pkg.coins + pkg.bonus} coin - ${pkg.priceAzn.toFixed(2)} AZN`,
      [{ text: 'Tamam' }]
    );
  };

  if (isLoading) return <LoadingSkeleton variant="default" />;
  if (error) return <ErrorState message="Balans yuklenmedi" onRetry={refetch} />;

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="flex-row items-center justify-between px-4 pt-12 pb-4">
        <Pressable
          onPress={() => router.back()}
          className="py-2 pr-4"
          accessibilityRole="button"
          accessibilityLabel="Geri"
        >
          <Text className="text-accent text-base font-medium">Geri</Text>
        </Pressable>
        <Text className="text-lg font-semibold text-primary dark:text-white">
          Coin Mağazası
        </Text>
        <View className="w-12" />
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: 40 }}
      >
        {/* Balance Card */}
        <View className="mx-4 mb-6 bg-primary rounded-2xl p-6 items-center">
          <Text className="text-sm text-gray-300 mb-1">Cari balans</Text>
          <Text className="text-4xl font-bold text-white mb-1">
            {balance?.coinBalance ?? 0}
          </Text>
          <Text className="text-sm text-gray-300">coin</Text>
        </View>

        {/* Packages */}
        <Text className="text-lg font-semibold text-primary dark:text-white px-4 mb-3">
          Coin Paketləri
        </Text>

        {PACKAGES.map((pkg) => (
          <Pressable
            key={pkg.id}
            onPress={() => handlePurchase(pkg)}
            className={`mx-4 mb-3 rounded-2xl p-4 border-2 ${
              pkg.highlight
                ? 'bg-accent/5 border-accent'
                : 'bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700'
            }`}
            accessibilityRole="button"
            accessibilityLabel={`${pkg.nameAz}: ${pkg.coins} coin, ${pkg.priceAzn} AZN`}
          >
            {pkg.highlight && (
              <View className="bg-accent rounded-full px-2 py-0.5 self-start mb-2">
                <Text className="text-xs text-white font-semibold">
                  Populyar
                </Text>
              </View>
            )}

            <View className="flex-row items-center justify-between">
              <View>
                <Text className="text-base font-semibold text-primary dark:text-white">
                  {pkg.nameAz}
                </Text>
                <View className="flex-row items-center mt-1">
                  <Text className="text-lg font-bold text-accent">
                    {pkg.coins.toLocaleString()}
                  </Text>
                  {pkg.bonus > 0 && (
                    <Text className="text-sm text-success font-medium ml-2">
                      +{pkg.bonus} bonus
                    </Text>
                  )}
                </View>
              </View>
              <View className="bg-accent rounded-xl px-4 py-2">
                <Text className="text-white font-bold text-base">
                  {pkg.priceAzn.toFixed(2)} {'₼'}
                </Text>
              </View>
            </View>
          </Pressable>
        ))}

        {/* Info */}
        <View className="px-4 mt-4">
          <Text className="text-xs text-gray-400 text-center">
            Coinlər hediyyə göndərmək və oyun xüsusiyyətləri üçün istifadə
            olunur. Alışlar geri qaytarıla bilməz.
          </Text>
        </View>
      </ScrollView>
    </View>
  );
}
