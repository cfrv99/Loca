# Loca — Complete Feature Specification (from PRD v2.0)
# Claude Code: READ THIS before implementing ANY feature. This is the source of truth.

---

## 1. Authentication & Onboarding

### 1.1 Login
- Google OAuth 2.0 + Apple Sign-In only (no email/password)
- JWT access token (60min) + refresh token (30 days, rotation on use)
- Biometric (Face ID / Fingerprint) for subsequent logins
- 18+ age verification at registration (date of birth check)

### 1.2 Onboarding (5 steps, required before app use)
1. **Basic Info**: Ad, yaş (auto from DOB), cins, profil foto (Google/Apple-dən import və ya manual upload)
2. **Maraqlar** (multi-select): Musiqi, Yemək, Sport, Səyahət, Texnologiya, Sənət, Gecə Həyatı, İş/Networking
3. **Məqsəd** (multi-select): Tanışlıq, Dostluq, Networking, Əyləncə
4. **Vibe Preference** (slider 0-100 for each): Romantic, Party, Chill, Adventurous — used for AI matching
5. **Privacy**: Default visibility (public/anonymous), push notification preferences

---

## 2. Venue Discovery

### 2.1 Home Screen (Discover Tab)
- Venue cards sorted by distance (nearest first)
- Each card shows:
  - Venue name + cover photo
  - Distance in meters/km
  - **Real-time people count**: total, male count, female count
  - **Activity indicator**: 🟢 green (>20 people), 🟡 yellow (5-20), ⚪ gray (<5)
  - Active games count (if any)
  - **FOMO**: "Chat-da nə baş verir?" — chat content is HIDDEN until check-in. Shows only message count.
- Pull-to-refresh, infinite scroll (cursor pagination)
- Filter: category (restaurant/bar/cafe/beach club), distance, activity level

### 2.2 Venue Detail
- Full info: name, address, phone, website, working hours, Google rating
- Cover photo gallery (swipeable)
- Real-time stats: people now, total today, peak hours chart
- **Event calendar**: upcoming venue events (if any)
- Map with venue pin
- **"QR Scan et və qoşul!"** CTA button (primary, large, animated)

### 2.3 Map View
- Google Maps with venue markers
- Marker size = activity level (bigger = more people)
- Marker color = activity (green/yellow/gray)
- Tap marker → venue detail bottom sheet
- User location blue dot

---

## 3. QR Check-in & Geofence

### 3.1 QR System
- Each venue has a `qr_secret_key` (generated at venue onboarding)
- QR payload = TOTP(secret_key, time_step=60s) — rotates every 60 seconds
- Venue tablet displays QR (PWA, auto-refresh every 55s)
- Tolerance: accept current + previous TOTP window (120s total)

### 3.2 Check-in Flow
1. User taps "QR Scan" → camera opens (expo-camera barcode scanner)
2. Scans QR → sends POST /checkin {qr_payload, lat, lng, device_fingerprint}
3. Backend validates: QR valid? → Geofence check (PostGIS ST_DWithin) → GPS spoof check → Rate limit (5min)
4. Success → user joins venue Social Hub
5. Fail → error message: "QR etibarsızdır" / "Məkandan kənardasan (150m)" / "5 dəqiqə gözləyin"

### 3.3 Check-out
- Manual: "Çıx" button in Social Hub
- Auto: background job checks every 2 min — if user outside geofence for 10 consecutive minutes → auto checkout
- On checkout: decrement Redis counter, remove from active users, create ghost (Phase 3)

### 3.4 Anti-Fraud
- QR rotates every 60s (can't screenshot and share)
- GPS spoofing: check `Settings.Secure.ALLOW_MOCK_LOCATION` on Android, trust iOS
- Device fingerprint: same device can't check-in to different venues simultaneously
- Rate limit: same user + same venue = 5 min cooldown

---

## 4. Venue Social Hub (Core Experience)

### 4.1 Hub Structure
After check-in, user enters the Social Hub with tabs:
- **Chat** — public venue chat (default tab)
- **People** — who's here
- **Feed** — venue memories/posts
- **Games** — active games + create

### 4.2 Public Chat
- Real-time messaging via SignalR (VenueChatHub)
- Message types:
  - **Text**: max 1000 chars
  - **Emoji**: standard Unicode
  - **GIF**: Giphy API integration (search + trending)
  - **Image**: photo from camera or gallery (max 5MB, auto-resize)
  - **Voice**: record up to 30 seconds, playback with waveform visualization
  - **System**: "Nigar qoşuldu", "Oyun başladı", "Gift göndərildi"
  - **Game invite**: inline game invitation card
  - **Gift**: inline gift animation trigger
- **Reply/Thread**: long-press message → "Cavab ver" → reply shows original message preview
- **Reactions**: long-press → emoji picker (❤️ 😂 😮 👏 🔥 😢) → reaction count under message
- **Pinned message**: venue admin can pin 1 message at top
- Anonymous users show as "Guest_XXXX" (random 4-digit)
- Auto-moderation: keyword filter (söyüş siyahısı) + AI toxicity classifier
- Message persistence: PostgreSQL + Redis cache (last 100)
- Load more: scroll up → cursor pagination → older messages

### 4.3 People Browser
- Grid layout: avatar, name, age, 2-3 interest badges
- Anonymous users HIDDEN (only counted in total)
- Tap → other user's profile:
  - Bio, interests, purposes
  - **Past venues**: list of venues they've checked into (with dates)
  - **Memories**: their posts/photos from this and other venues
  - "Match Request" button (if both non-anonymous, same venue)

### 4.4 Venue Feed (Social Wall)
- TikTok-style vertical scroll (full-screen cards, swipe up for next)
- Post types: photo + caption, short video (15s), status text
- Each post shows: author avatar+name, timestamp, like count, comment count
- **Like**: heart button, toggle, optimistic UI update
- **Comment**: bottom sheet, text only, newest first
- **"Xatirə" (Memory)**: posts PERSIST after user leaves. Venue feed = all-time memories from that venue.
- Past visitors can see memories from venues they visited before
- Post creation: fab button → camera/gallery → caption → post (auto-tagged to current venue)

### 4.5 Push Notifications
- FCM (Android) + APNs (iOS) via expo-notifications
- Notification types:
  - "Yeni mesaj: {sender} wrote in {venue} chat"
  - "Match request from {name}"
  - "Match accepted! Start chatting"
  - "{name} sent you a {gift}!"
  - "Vibe Bomb received! Guess who?"
  - "Game starting in {venue}: {game_type}"
  - "Time Capsule unlocked!"
- Device token registration on login, refresh on each app open

---

## 5. Interactive Games

### 5.1 Game System
- Any checked-in user can create a game from the Games tab
- Creator = HOST (controls settings, starts game)
- Lobby: shows player avatars, ready status, min/max players, countdown
- Reconnection: 60s grace period on disconnect, game pauses for that player
- Result: auto-posted to venue public chat + leaderboard entry

### 5.2 Mafia (5-12 players)
- **Roles**: Mafia (kills at night), Doctor (saves), Detective (investigates), Citizen (votes)
- **Role distribution**: 5p=1M/1D/1Det/2C, 6p=2M/1D/1Det/2C, 8p=2M/1D/1Det/4C, 10p=3M/1D/1Det/5C, 12p=3M/1D/1Det/7C
- **Phases**: Night (45s) → Day Discussion (120s) → Day Vote (60s) → Elimination → repeat
- **Night**: Mafia picks target (majority vote if 2+ mafia), Doctor picks save, Detective picks investigate
- **Day**: Discussion (text chat within game), then voting — most votes eliminated
- **Win**: Mafia wins if mafia >= citizens. Citizens win if all mafia eliminated.
- **Fog of war**: Players only see their own role + public eliminations. Mafia see each other.
- **Timeout**: if player doesn't act in time → random action (night) or skip (vote)

### 5.3 Truth or Dare (3-10 players)
- Turn-based: spinner selects random player
- Selected player chooses: Truth or Dare
- System shows random question/challenge from database (200+ items in Azerbaijani)
- **Timer**: 60s to complete (for Dare) or answer (for Truth)
- **Skip**: costs 50 coins (prevents abuse of skip)
- **Custom**: any player can submit custom question (host approves)
- **Categories**: Funny, Romantic, Extreme, Intellectual

### 5.4 Uno (2-6 players)
- Standard Uno rules: match color or number
- Special cards: Skip, Reverse, Draw Two (+2), Wild, Wild Draw Four (+4)
- Color picker popup when Wild played
- Can stack +2 on +2, +4 on +4 (house rule, configurable by host)
- "UNO!" button — if you forget to press when 1 card left, anyone can catch you (+2 penalty)
- Win: first to empty hand. Points = sum of other players' remaining cards.

### 5.5 Domino (2-4 players)
- Standard 28-tile double-six set
- Draw from boneyard if can't play
- Game ends when someone empties hand OR all players blocked
- Scoring: winner gets sum of all opponents' remaining pips

### 5.6 Quiz Battle (2-20 players)
- 10 rounds, each round = 1 question + 4 answers
- Timer: 15s per question
- Faster correct answer = more points (15s=100pts, 10s=80pts, 5s=60pts, last second=40pts)
- Categories: Ümumi Bilik, Azərbaycan, İdman, Musiqi, Texnologiya, Tarix
- 500+ questions database in Azerbaijani

### 5.7 Would You Rather (2+ players)
- Infinite rounds until host stops
- Each round: 2 options shown, all players vote anonymously
- Results shown: percentage split + who voted what (after reveal)
- No winner — engagement/fun feature
- 200+ question pairs in Azerbaijani

---

## 6. Matching & Private Chat

### 6.1 Match Request Rules
- ONLY between users at the SAME venue (both geofence-verified)
- ONLY non-anonymous users can send/receive
- Free limit: 5 match requests per day (reset at midnight UTC+4 Baku time)
- Optional intro message (max 200 chars)
- Push notification to receiver

### 6.2 Match Lifecycle
1. Sender sends request → status: `pending`
2. Receiver sees in inbox → Accept or Decline
3. Accept → Conversation created → private chat opens → status: `accepted`
4. Decline → 24h cooldown before sender can re-request same person → status: `declined`
5. No response in 48h → auto-expire → status: `expired`

### 6.3 Spam Protection
- 3 declines from same receiver → sender blocked from that person for 48h
- Report system: reason (harassment, spam, fake profile, inappropriate content) + description
- Block: hides user from ALL interactions (chat, people list, matching, games)

### 6.4 Private Chat
- Same message types as public chat (text, emoji, GIF, image, voice, game invite, gift)
- **Typing indicator**: real-time "typing..." shown
- **Read receipts**: double checkmark when read
- **Online status**: green dot = online, gray = offline, shows "last seen X min ago"
- Chat persists even after both leave the venue
- Game invitation: can send 1v1 game invite within private chat
- Gift sending: same gift system as public chat, but only recipient sees animation

---

## 7. Gifting & Virtual Economy

### 7.1 Coin Packages (IAP)
| Package | Price (AZN) | Coins | Bonus |
|---------|-------------|-------|-------|
| İlk Alış | 2.99 | 300 | +50 |
| Standard | 9.99 | 1,100 | +100 |
| Premium | 24.99 | 3,000 | +500 |
| VIP | 49.99 | 7,000 | +1,500 |

### 7.2 Gift Tiers
| Tier | Cost | Examples |
|------|------|---------|
| Basic (10-50 coin) | 10, 20, 30, 50 | Gül 🌹, Ürək ❤️, Like 👍, Emoji Blast 🎉 |
| Premium (100-500 coin) | 100, 200, 300, 500 | Kokteyl 🍸, Pizza 🍕, Atəşfəşanlıq 🎆, Tac 👑 |
| Luxury (1000-5000 coin) | 1000, 2000, 5000 | Avtomobil 🏎️, Villa 🏠, Qızıl Üzük 💍, Helikopter 🚁 |
| Venue-specific | varies | Custom per venue (beach club → virtual surfboard) |

### 7.3 Gift Behavior
- **Public chat**: gift animation plays for EVERYONE in the venue chat (social status effect)
- **Private chat**: animation plays for ONLY sender + receiver
- **Animation**: Lottie full-screen overlay, 2-4 seconds, auto-dismiss
- **Profile badge**: "Bu həftə ən çox gift alan" — shown on profile for top receiver per venue

### 7.4 Loca Gold (Premium Plan — 9.99 AZN/month)
| Feature | Free | Gold |
|---------|------|------|
| Match request/gün | 5 | Unlimited |
| AI Vibe Match/gün | 1 | Unlimited |
| Kim baxdı (profile views) | Yoxdur | Var |
| Priority matching | Yoxdur | Var |
| Custom avatar frame | Yoxdur | Gold frame |
| Ad-free | Yoxdur | Var |

---

## 8. Vibe AI Radar (Phase 2)

### 8.1 Venue Vibe Score
- Calculated from: check-in patterns (time of day, day of week), chat sentiment analysis, game types played, gift types sent
- Output: Romantic %, Party %, Chill %, Adventurous % (must sum to 100)
- Updates every 30 minutes

### 8.2 AI Matching
- Input: user's onboarding vibe preferences + behavioral data (venues visited, games played, gifts sent, chat activity)
- Algorithm: Phase 1 (cold start) → rule-based from onboarding | Phase 2 (1000+ users) → collaborative filtering (scikit-learn) | Phase 3 (5000+) → deep learning (PyTorch)
- Output: top 3 most compatible people in current venue, shown as anonymous avatars + match % (e.g., "87% uyğun")
- "Vibe Match" button → if both tap → instant private chat opens (no match request needed)

---

## 9. Secret Admirer & Vibe Bomb (Phase 2)

### 9.1 Vibe Bomb Flow
1. User A in venue → taps "Vibe Bomb" on User B's profile → pays 100 coins
2. User B gets push: "Kimsə sənə Vibe Bomb göndərdi! 💣"
3. B gets 3 hints over 24 hours:
   - Hint 1 (immediately): "O da {game} oynadı bu gün" (from sender's recent activity)
   - Hint 2 (after 6h): "Son 30 dəqiqə ərzində check-in etdi"
   - Hint 3 (after 12h): "{N} ortaq marağınız var" (shared interests count)
4. B gets 3 guess attempts (select from people list in venue)
5. Correct guess → "Reveal Chat" opens + both get 50 bonus coins
6. Wrong guesses → after 3 fails, bomb expires (sender's coins NOT refunded)
7. 24h timeout → bomb expires regardless

---

## 10. Collaborative Playlist & DJ (Phase 3)

### 10.1 System
- Spotify API integration (OAuth + Web API)
- Any checked-in user can search Spotify and submit a "Song Request" (costs 20 coins)
- Request goes to venue queue
- Other users can upvote/downvote requests (1 vote per user per song)
- Queue sorted by net votes (highest first)
- Venue admin (DJ panel) can approve/reject/skip any request
- "Now Playing" status shown in Music tab

---

## 11. Cross-Venue Chain Party (Phase 3)

### 11.1 System
- When 2+ matched users are at the same venue, they can receive a "Chain Party" invitation
- Challenge: check-in to 3 different partner venues within 4 hours
- Progress tracker shows: ✅ Venue 1 → 🔲 Venue 2 (800m) → 🔲 Venue 3
- On 3/3 completion: special group game unlocked + 500 bonus coins each
- Partner venues can offer discounts (configured in admin panel)
- Leaderboard: "Bu həftə ən çox chain tamamlayan"

---

## 12. Memory Time Capsule (Phase 3)

### 12.1 System
- On check-in, user can create a capsule: select 1-3 photos + write a message
- Capsule "sealed" with animation → locked for 1 year
- Daily cron job checks: any capsules reaching unlock date?
- Push: "Xatırlayırsan? Bu gün tam 1 il əvvəl {venue}-da idin"
- Unlock screen: reveal animation + photos + message + option to share on social media
- Premium (Gold): video capsule (60s) + music slideshow

---

## 13. Venue Admin Panel (B2B)

### 13.1 Features
- **Dashboard**: real-time check-in count, hourly chart, demographics (age/gender), peak hours
- **Venue management**: edit info, upload photos, working hours, geofence radius
- **QR management**: display current QR, download for print, rotation status
- **Event management**: create/edit/delete events, push notification to nearby users
- **Chat moderation**: view public chat, pin message, ban/mute user
- **Custom gifts**: create venue-specific gifts (icon upload, set coin price, animation)
- **Analytics**: daily/weekly/monthly reports, top users, retention, average stay duration
- **Promoted placement**: pay for top position in Discover feed (CPC model)
- **Chain Party**: create partnership with other venues, set discount offers

### 13.2 Access
- Web app (Vite + React) at admin.loca.az
- Login with venue owner credentials
- Multi-venue support (one owner, multiple venues)
