import { useState } from 'react';
import { View, Text, Pressable } from 'react-native';
import { useRouter } from 'expo-router';
import { useVenueStore } from '../../../shared/stores/venue-store';
import { EmptyState } from '../../../shared/components/empty-state';
import { PublicChatScreen } from '../../../features/chat/screens/public-chat';
import { PeopleGrid } from '../../../features/venue/components/people-grid';
import { VenueFeed } from '../../../features/venue/components/venue-feed';
import { GamesList } from '../../../features/venue/components/games-list';

type HubTab = 'chat' | 'people' | 'feed' | 'games';

const HUB_TABS: { key: HubTab; label: string }[] = [
  { key: 'chat', label: 'Chat' },
  { key: 'people', label: 'Insanlar' },
  { key: 'feed', label: 'Lent' },
  { key: 'games', label: 'Oyunlar' },
];

export default function HubScreen() {
  const router = useRouter();
  const { isCheckedIn, currentVenueId, currentVenueName, clearCheckIn } =
    useVenueStore();
  const [activeTab, setActiveTab] = useState<HubTab>('chat');

  // Not checked in: prompt to scan QR
  if (!isCheckedIn || !currentVenueId) {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark">
        <View className="px-4 pt-12 pb-2">
          <Text className="text-3xl font-bold text-primary dark:text-white">
            Hub
          </Text>
        </View>
        <View className="flex-1 items-center justify-center px-8">
          <Text className="text-5xl mb-4">📱</Text>
          <Text className="text-lg text-gray-500 dark:text-gray-400 text-center mb-6">
            QR scan edərək bir məkana qoşulun
          </Text>
          <Pressable
            onPress={() => router.push('/(tabs)/discover')}
            className="bg-accent rounded-xl py-3 px-8"
            accessibilityRole="button"
            accessibilityLabel="Məkanları kəşf et"
          >
            <Text className="text-white font-semibold text-base">
              Kəşf et
            </Text>
          </Pressable>
        </View>
      </View>
    );
  }

  const renderTabContent = () => {
    switch (activeTab) {
      case 'chat':
        return <PublicChatScreen venueId={currentVenueId} />;
      case 'people':
        return <PeopleGrid venueId={currentVenueId} />;
      case 'feed':
        return <VenueFeed venueId={currentVenueId} />;
      case 'games':
        return <GamesList venueId={currentVenueId} />;
      default:
        return null;
    }
  };

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Header */}
      <View className="px-4 pt-12 pb-2">
        <View className="flex-row items-center justify-between">
          <View className="flex-1">
            <Text className="text-xl font-bold text-primary dark:text-white">
              {currentVenueName}
            </Text>
            <Text className="text-xs text-success font-medium">Check-in aktiv</Text>
          </View>
          <Pressable
            onPress={clearCheckIn}
            className="bg-error/10 rounded-full px-3 py-1.5"
            accessibilityRole="button"
            accessibilityLabel="Cix"
          >
            <Text className="text-error text-xs font-medium">Cix</Text>
          </Pressable>
        </View>
      </View>

      {/* Tab Bar */}
      <View className="flex-row px-2 border-b border-gray-200 dark:border-gray-700">
        {HUB_TABS.map((tab) => (
          <Pressable
            key={tab.key}
            onPress={() => setActiveTab(tab.key)}
            className={`flex-1 py-3 items-center border-b-2 ${
              activeTab === tab.key
                ? 'border-accent'
                : 'border-transparent'
            }`}
            accessibilityRole="tab"
            accessibilityLabel={tab.label}
            accessibilityState={{ selected: activeTab === tab.key }}
          >
            <Text
              className={`text-sm font-medium ${
                activeTab === tab.key
                  ? 'text-accent'
                  : 'text-gray-500 dark:text-gray-400'
              }`}
            >
              {tab.label}
            </Text>
          </Pressable>
        ))}
      </View>

      {/* Tab Content */}
      <View className="flex-1">{renderTabContent()}</View>
    </View>
  );
}
