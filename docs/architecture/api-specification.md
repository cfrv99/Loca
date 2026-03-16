# Loca REST API Specification

Base URL: `/api/v1`
Auth: JWT Bearer on all endpoints except POST /auth/*
Response wrapper: `{ success: bool, data: T?, error: { code: string, message: string }? }`

---

## Auth Endpoints

### POST /auth/google
Login/register via Google OAuth
```
Request:  { idToken: string }
Response: { accessToken: string, refreshToken: string, user: UserDto, isNewUser: bool }
```

### POST /auth/apple
Login/register via Apple Sign-In
```
Request:  { identityToken: string, authorizationCode: string, fullName?: string }
Response: { accessToken: string, refreshToken: string, user: UserDto, isNewUser: bool }
```

### POST /auth/refresh
Refresh access token (old refresh token invalidated)
```
Request:  { refreshToken: string }
Response: { accessToken: string, refreshToken: string }
Errors:   401 INVALID_REFRESH_TOKEN
```

---

## User Endpoints

### GET /users/me
Get current user profile
```
Response: UserDto { id, email, displayName, avatarUrl, dateOfBirth, gender, 
           interests[], purposes[], vibePreferences[], privacySettings, 
           isOnboarded, isPremium, coinBalance, createdAt }
```

### PUT /users/me
Update profile (partial update)
```
Request:  { displayName?, bio?, interests[]?, purposes[]?, vibePreferences[]?, privacySettings? }
Response: UserDto
```

### POST /users/me/avatar
Upload profile photo
```
Request:  multipart/form-data { file: image (max 5MB, jpg/png) }
Response: { avatarUrl: string, thumbnailUrl: string }
```

### PUT /users/me/onboarding
Complete onboarding (one-time)
```
Request:  { interests: string[], purposes: string[], vibePreferences: {vibe:string, weight:number}[], privacySettings: {defaultAnonymous: bool, pushEnabled: bool} }
Response: UserDto (isOnboarded = true)
```

### GET /users/{id}
Get other user's public profile
```
Response: PublicUserDto { id, displayName, avatarUrl, age, gender, interests[], 
           purposes[], pastVenues[{venueId, venueName, lastVisit}], 
           memoriesCount, giftBadge? }
Errors:   404 USER_NOT_FOUND, 403 USER_BLOCKED
```

---

## Venue Endpoints

### GET /venues/nearby
Discover venues sorted by distance
```
Query:    lat, lng, radius (default 5000m), category?, cursor?, pageSize (default 20)
Response: CursorPage<VenueCardDto> { items[{id, name, coverPhotoUrl, address, category,
           distanceMeters, stats:{total, male, female}, activityLevel, activeGames, 
           chatMessageCount}], nextCursor, hasMore }
```

### GET /venues/{id}
Venue detail
```
Response: VenueDetailDto { id, name, description, address, category, lat, lng, 
           coverPhotoUrl, photoUrls[], googleRating, phone, website, 
           workingHours, stats, events[], geofenceRadius }
```

### GET /venues/{id}/stats
Real-time stats (Redis)
```
Response: { total: int, male: int, female: int, activeGames: int, chatMessages24h: int }
```

### GET /venues/{id}/people
People at venue (non-anonymous only)
```
Query:    cursor?, pageSize
Response: CursorPage<ActiveUserDto> { items[{userId, displayName, avatarUrl, age, interests[]}] }
```

### GET /venues/{id}/feed
Venue memory feed
```
Query:    cursor?, pageSize
Response: CursorPage<PostDto> { items[{id, userId, userName, userAvatar, content, 
           mediaUrls[], likeCount, commentCount, isLikedByMe, createdAt}] }
```

### GET /venues/{id}/messages
Chat history (for initial load + scroll-up pagination)
```
Query:    cursor?, pageSize (default 50)
Response: CursorPage<ChatMessageDto>
```

---

## Check-in Endpoints

### POST /checkin
Check in to venue via QR
```
Request:  { qrPayload: string, lat: number, lng: number, deviceFingerprint: string, isAnonymous: bool }
Response: { checkInId: string, venueId: string, venueName: string, 
           hubConnectionInfo: { chatHubUrl, gameHubUrl } }
Errors:   400 INVALID_QR, 400 OUTSIDE_GEOFENCE (with distanceMeters), 429 RATE_LIMITED
```

### POST /checkout
Manual checkout
```
Request:  { checkInId: string }
Response: { success: true, duration: string }
```

### GET /venues/{id}/qr
Get current QR payload for venue tablet (venue admin only)
```
Response: { qrPayload: string, expiresInSeconds: int }
```

---

## Feed Endpoints

### POST /posts
Create venue post
```
Request:  multipart { venueId: string, content?: string, media?: file[] (max 3, max 5MB each) }
Response: PostDto
```

### POST /posts/{id}/like
Toggle like
```
Response: { liked: bool, likeCount: int }
```

### GET /posts/{id}/comments
```
Query:    cursor?, pageSize
Response: CursorPage<CommentDto> { items[{id, userId, userName, userAvatar, content, createdAt}] }
```

### POST /posts/{id}/comments
```
Request:  { content: string (max 500 chars) }
Response: CommentDto
```

---

## Match Endpoints

### POST /matches/request
Send match request
```
Request:  { receiverId: string, introMessage?: string (max 200 chars) }
Response: MatchRequestDto { id, status: "pending" }
Errors:   400 SAME_VENUE_REQUIRED, 400 ANONYMOUS_NOT_ALLOWED, 429 DAILY_LIMIT_REACHED, 
           403 USER_BLOCKED, 400 ALREADY_PENDING
```

### PUT /matches/{id}/respond
Accept or decline
```
Request:  { action: "accept" | "decline" }
Response: { conversationId?: string (if accepted) }
```

### GET /matches/inbox
```
Query:    status? (pending/accepted/declined), cursor?, pageSize
Response: CursorPage<MatchRequestDto>
```

### GET /conversations
List active conversations
```
Response: [{conversationId, otherUser: PublicUserDto, lastMessage: MessageDto, unreadCount, updatedAt}]
```

---

## Economy Endpoints

### GET /economy/balance
```
Response: { coinBalance: int, totalPurchased: int, totalSpent: int }
```

### POST /economy/purchase
Validate IAP receipt and credit coins
```
Request:  { platform: "ios"|"android", receiptData: string, productId: string }
Response: { coinsAdded: int, newBalance: int, transactionId: string }
Errors:   400 INVALID_RECEIPT, 400 ALREADY_PROCESSED
```

### GET /economy/gifts
Gift catalog
```
Response: GiftDto[] { id, name, nameAz, tier, coinPrice, iconUrl, animationUrl, venueId? }
```

### POST /economy/gifts/send
Send gift
```
Request:  { giftId: string, recipientId: string, context: "public_chat"|"private_chat", 
           venueId?: string, conversationId?: string }
Response: { transactionId, newBalance }
Errors:   400 INSUFFICIENT_BALANCE
```

---

## Report/Block Endpoints

### POST /users/{id}/report
```
Request:  { reason: "harassment"|"spam"|"fake"|"inappropriate"|"other", description?: string }
Response: { reportId }
```

### POST /users/{id}/block
```
Response: { blocked: true }
```

### DELETE /users/{id}/block
Unblock
```
Response: { blocked: false }
```

---

## Notification Endpoints

### POST /notifications/device-token
Register FCM/APNs token
```
Request:  { token: string, platform: "ios"|"android" }
Response: { registered: true }
```

---

## Vibe AI Endpoints (Phase 2)

### GET /venues/{id}/vibe
```
Response: { romantic: int, party: int, chill: int, adventurous: int } (percentages, sum=100)
```

### GET /venues/{id}/vibe-matches
```
Response: [{anonymousAvatar, matchPercent, vibeOverlap[]}] (max 3)
```

### POST /vibe-match/{matchId}/accept
```
Response: { conversationId? } (if mutual)
```

---

## Vibe Bomb Endpoints (Phase 2)

### POST /vibe-bombs/send
```
Request:  { receiverId: string, venueId: string }
Response: { bombId, coinDeducted: 100 }
```

### GET /vibe-bombs/received
```
Response: [{id, hints[], guessesRemaining, expiresAt}]
```

### POST /vibe-bombs/{id}/guess
```
Request:  { guessedUserId: string }
Response: { correct: bool, revealChatConversationId?: string, bonusCoins?: int }
```
