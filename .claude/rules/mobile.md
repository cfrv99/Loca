# Mobile Development Rules (React Native Expo)

## Why Expo (not bare RN CLI)
- Expo Go enables real-time iOS testing without Mac build each time
- EAS Build handles native builds when needed (push notifications, deep links)
- expo-dev-client for custom native modules if AR is added later (Phase 3)
- OTA updates via EAS Update for instant bug fixes without App Store review

## Project Structure
```
apps/mobile/
├── app/                    # App entry, navigation, providers
│   ├── _layout.tsx         # Root layout (Expo Router)
│   ├── (auth)/             # Auth flow screens
│   ├── (tabs)/             # Main tab navigator
│   │   ├── discover/       # Venue discovery screens
│   │   ├── hub/            # Venue social hub (chat, games, people)
│   │   ├── matches/        # Matching + private chats
│   │   └── profile/        # User profile + settings
│   └── venue/[id]/         # Dynamic venue routes
├── features/               # Feature modules
│   ├── auth/               # Login, register, onboarding
│   ├── venue/              # Venue discovery, detail, check-in
│   ├── chat/               # Public chat, private chat
│   ├── games/              # Game lobby, game screens
│   ├── matching/           # Match requests, AI matching
│   ├── gifting/            # Gift shop, animations, coin purchase
│   └── profile/            # Profile view, edit, memories
├── shared/
│   ├── components/         # Reusable UI components
│   ├── hooks/              # Custom hooks
│   ├── services/           # API client, SignalR client, storage
│   ├── stores/             # Zustand stores
│   ├── utils/              # Helpers, formatters, validators
│   └── constants/          # Colors, dimensions, config
├── assets/                 # Images, fonts, Lottie animations
└── __tests__/              # Jest test files
```

## State Management
- **Zustand**: Auth state, user preferences, UI state (modals, tabs)
- **React Query (TanStack)**: All server data (venues, profiles, messages, games)
- **SignalR state**: Managed via custom hook `useSignalR()` with auto-reconnect
- NEVER use React Context for global state (performance issues with frequent updates)

## Navigation
- Expo Router (file-based routing)
- Deep linking: `loca://venue/{id}`, `loca://match/{id}`, `loca://profile/{id}`
- Auth guard: redirect to login if no valid token

## Styling
- NativeWind (Tailwind CSS for React Native) for all styling
- Design tokens in `tailwind.config.js` matching design system
- Dark mode support from day 1 (use `dark:` prefix)
- NO inline style objects unless dynamic values required

## Performance
- FlatList with `getItemLayout` for all scrollable lists
- Image optimization: expo-image with caching + progressive loading
- Lazy load screens with React.lazy + Suspense
- SignalR: debounce typing indicators, batch message updates
- Animations: Reanimated 3 for gesture-driven, Lottie for gift animations

## Offline Handling
- React Query offline mode for venue list, profile data
- Queue match requests and messages when offline, sync on reconnect
- Show clear offline indicator in UI
