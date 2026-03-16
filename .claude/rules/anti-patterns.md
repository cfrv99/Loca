# Anti-Patterns — NEVER do these

## Backend
- ❌ NEVER throw exceptions for business logic → use Result<T>
- ❌ NEVER return raw entity from controller → always map to DTO
- ❌ NEVER use data annotations on domain entities → Fluent API only
- ❌ NEVER put business logic in controllers → goes in Handler
- ❌ NEVER access DbContext directly from controller → use Repository
- ❌ NEVER hardcode connection strings → use IConfiguration
- ❌ NEVER use `.Result` or `.Wait()` on async calls → always await
- ❌ NEVER return Task from SignalR hub methods without async → causes deadlocks
- ❌ NEVER trust client-sent user IDs → extract from JWT token
- ❌ NEVER skip validation → every Command needs a Validator

## Mobile
- ❌ NEVER use ScrollView for dynamic lists → use FlatList
- ❌ NEVER use inline style objects → NativeWind classes only
- ❌ NEVER use React Context for frequently changing state → Zustand
- ❌ NEVER store tokens in AsyncStorage → use expo-secure-store
- ❌ NEVER skip loading/error states → every screen needs all 3
- ❌ NEVER use console.log in production code → use __DEV__ guard
- ❌ NEVER make API calls directly in components → use React Query hooks
- ❌ NEVER hardcode API URLs → use environment-based config
- ❌ NEVER skip accessibilityLabel on interactive elements
- ❌ NEVER use `any` type in TypeScript → always define proper types

## SignalR
- ❌ NEVER send full game state to all players → fog of war
- ❌ NEVER trust client-reported location → server validates with geofence
- ❌ NEVER skip reconnection handling → users lose connection constantly
- ❌ NEVER send large payloads → keep SignalR messages < 1KB

## General
- ❌ NEVER commit without running tests first
- ❌ NEVER create a feature without at least 1 test
- ❌ NEVER modify database schema without creating a migration
- ❌ NEVER skip the Definition of Done checklist
