import { Tabs } from 'expo-router';
import { Text } from 'react-native';

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: '#3B82F6',
        tabBarInactiveTintColor: '#6B7280',
        tabBarStyle: {
          backgroundColor: '#FFFFFF',
          borderTopColor: '#E5E7EB',
          paddingBottom: 8,
          paddingTop: 8,
          height: 64,
        },
        tabBarLabelStyle: {
          fontSize: 11,
          fontWeight: '600',
        },
      }}
    >
      <Tabs.Screen
        name="discover"
        options={{
          title: 'Kesf et',
          tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 20 }}>K</Text>,
        }}
      />
      <Tabs.Screen
        name="hub"
        options={{
          title: 'Hub',
          tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 20 }}>H</Text>,
        }}
      />
      <Tabs.Screen
        name="matches"
        options={{
          title: 'Maclar',
          tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 20 }}>M</Text>,
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Profil',
          tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 20 }}>P</Text>,
        }}
      />
    </Tabs>
  );
}
