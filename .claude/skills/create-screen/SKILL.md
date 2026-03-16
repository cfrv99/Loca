---
name: create-screen
description: Use when creating a new React Native Expo screen. Generates screen component, hooks, API integration, tests, and navigation setup following project conventions.
---

# Create Screen Skill

## Step 1: Plan the Screen
Before writing code, answer:
- What data does this screen need? (React Query + API endpoint)
- What user interactions exist? (buttons, inputs, gestures)
- Does it need real-time data? (SignalR subscription)
- What loading/error/empty states are needed?

## Step 2: Create Feature Module
```
features/{feature-name}/
├── screens/
│   └── {screen-name}-screen.tsx   # Main screen component
├── components/
│   └── {component-name}.tsx       # Screen-specific components
├── hooks/
│   ├── use-{feature}-query.ts     # React Query hooks
│   └── use-{feature}-signalr.ts   # SignalR hooks (if real-time)
├── types.ts                       # Feature-specific types
└── __tests__/
    └── {screen-name}-screen.test.tsx
```

## Step 3: Screen Component Template
```tsx
import { View, Text, FlatList } from 'react-native';
import { useLocalSearchParams } from 'expo-router';
// Feature hooks
import { useVenueQuery } from '../hooks/use-venue-query';

export default function VenueDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { data, isLoading, error } = useVenueQuery(id);

  if (isLoading) return <LoadingSkeleton />;    // ALWAYS handle loading
  if (error) return <ErrorState retry={refetch} />;  // ALWAYS handle error
  if (!data) return <EmptyState />;             // ALWAYS handle empty

  return (
    <View className="flex-1 bg-background-light dark:bg-background-dark">
      {/* Screen content */}
    </View>
  );
}
```

## Step 4: React Query Hook
```tsx
import { useQuery, useMutation } from '@tanstack/react-query';
import { api } from '@/shared/services/api-client';

export function useVenueQuery(id: string) {
  return useQuery({
    queryKey: ['venue', id],
    queryFn: () => api.get(`/venues/${id}`).then(r => r.data.data),
    enabled: !!id,
    staleTime: 1000 * 60 * 2,  // 2 min
  });
}
```

## Step 5: SignalR Hook (if real-time)
```tsx
import { useSignalR } from '@/shared/hooks/use-signalr';

export function useVenueChatSignalR(venueId: string) {
  const { connection, isConnected } = useSignalR('/hubs/venue-chat');

  useEffect(() => {
    if (!connection || !isConnected) return;
    connection.invoke('JoinVenue', venueId);
    connection.on('receiveMessage', (msg) => { /* update state */ });
    return () => {
      connection.invoke('LeaveVenue', venueId);
      connection.off('receiveMessage');
    };
  }, [connection, isConnected, venueId]);
}
```

## Step 6: Tests (MANDATORY)
```tsx
import { render, screen, waitFor } from '@testing-library/react-native';
import VenueDetailScreen from '../screens/venue-detail-screen';

// Test 1: renders loading state
// Test 2: renders data correctly
// Test 3: handles error state
// Test 4: user interaction (button press, etc.)
```

## Step 7: Verify
```bash
cd apps/mobile
npx tsc --noEmit                    # Type check
npx jest features/{feature}/        # Run feature tests
npx expo start                      # Visual check in Expo Go
```

## Golden Examples to Match
- List/Discovery screens → match `docs/golden-examples/mobile/discover-screen.md`
- Chat/Real-time screens → match `docs/golden-examples/mobile/chat-screen.md`
- Read `docs/prd/feature-specs.md` for exact behavior specification
- Read `docs/content/azerbaijani-content.md` for UI strings

## Checklist Before Done
- [ ] Loading, error, and empty states handled
- [ ] NativeWind classes used (no inline styles)
- [ ] Dark mode supported (dark: prefix)
- [ ] Touch targets ≥ 44x44 points
- [ ] FlatList used for scrollable lists (not ScrollView)
- [ ] React Query for all server data
- [ ] At least 3 tests written
- [ ] `npx tsc --noEmit` passes
- [ ] Visually tested in Expo Go
