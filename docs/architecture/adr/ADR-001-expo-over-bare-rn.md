# ADR-001: React Native Expo over bare React Native CLI

**Status:** Accepted
**Date:** 2026-03-16
**Deciders:** @architect, @mobile-1, @mobile-2

## Context
Loca mobile app cross-platform (iOS + Android) olmalıdır. İki əsas seçim var: React Native CLI (bare workflow) və Expo (managed workflow). Əvvəlki PRD-də CLI seçilmişdi AR Ghost Mode səbəbindən, lakin AR Phase 3-dədir (ay 6-8) və ilk 5 ay Expo ilə development daha sürətli olacaq.

## Decision
React Native Expo (SDK 52) istifadə edirik. Phase 3-də AR lazım olduqda expo-dev-client ilə native module əlavə edəcəyik.

## Consequences

### Positive
- Expo Go ilə real-time iOS testing (Mac build olmadan)
- EAS Build ilə automated native builds (iOS + Android)
- EAS Update ilə OTA updates (App Store review olmadan bug fix)
- expo-camera, expo-location, expo-notifications — hazır, test olunmuş
- Development speed: ~30% daha sürətli MVP delivery
- Expo Router: file-based routing, deep linking out-of-box

### Negative
- Phase 3 AR üçün expo-dev-client lazım olacaq (Expo Go artıq işləməyəcək)
- Bəzi native module-lar (custom QR rotation display) config plugin tələb edə bilər
- Expo SDK upgrade-ləri bəzən breaking changes gətirir

### Risks
- AR library (ViroReact / expo-three) Expo ilə uyğunluq problemi ola bilər — mitigation: Phase 3 başlamazdan 2 həftə əvvəl spike/POC et

## Alternatives Considered

| Option | Pros | Cons | Why rejected |
|--------|------|------|-------------|
| React Native CLI (bare) | Full native access, AR ready | Slower dev, manual native config, no OTA | AR 6 ay sonradır, MVP speed daha vacibdir |
| Flutter | Good performance, single codebase | Team .NET/React expertise yoxdur, ecosystem kiçik | Tech stack mismatch |
| Native (Swift + Kotlin) | Best performance | 2x development cost, 2x team | Budget və vaxt məhdudiyyəti |
