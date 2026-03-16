# SignalR Hub Contracts — Source of Truth

Backend implements server methods (PascalCase). Mobile calls them and listens to client callbacks (camelCase).
ALL agents MUST read this before implementing any real-time feature.

---

## VenueChatHub — `/hubs/venue-chat`
**Auth:** JWT Bearer required

### Client → Server (mobile calls these)
| Method | Params | Description |
|--------|--------|-------------|
| `JoinVenue(venueId: string)` | venue UUID | Join venue chat room. Requires active check-in. Broadcasts `userJoined` to room. |
| `LeaveVenue(venueId: string)` | venue UUID | Leave venue chat room. Broadcasts `userLeft`. |
| `SendMessage(venueId: string, content: string, type: string, replyToId?: string, metadata?: object)` | | Send chat message. Types: text, image, gif, voice. Server validates, persists, broadcasts. |
| `SendReaction(messageId: string, emoji: string)` | | Add/toggle reaction on message. Emoji: ❤️😂😮👏🔥😢 |
| `StartTyping(venueId: string)` | | Broadcast typing indicator (debounce 3s client-side) |
| `StopTyping(venueId: string)` | | Clear typing indicator |

### Server → Client (mobile listens to these)
| Event | Payload | Description |
|-------|---------|-------------|
| `receiveMessage(msg: ChatMessageDto)` | `{id, senderId, senderName, senderAvatar, type, content, mediaUrl, replyTo?, metadata?, createdAt}` | New message in room |
| `userJoined(user: ActiveUserDto)` | `{userId, displayName, avatar, isAnonymous}` | Someone checked in |
| `userLeft(userId: string)` | | Someone checked out |
| `typingStarted(userId: string, displayName: string)` | | User is typing |
| `typingStopped(userId: string)` | | User stopped typing |
| `reactionUpdated(messageId: string, reactions: ReactionDto[])` | `[{emoji, count, userIds}]` | Reaction count changed |
| `messagePinned(msg: ChatMessageDto)` | | Admin pinned a message |
| `giftReceived(gift: GiftEventDto)` | `{senderId, senderName, giftName, giftTier, animationUrl, coinCost}` | Gift sent in chat — trigger full-screen Lottie |
| `activeUsersUpdated(count: VenueCountDto)` | `{total, male, female}` | People count changed |

---

## GameHub — `/hubs/game`
**Auth:** JWT Bearer required

### Client → Server
| Method | Params | Description |
|--------|--------|-------------|
| `CreateGame(venueId: string, gameType: string, maxPlayers: int, settings?: object)` | | Host creates game. Returns sessionId. |
| `JoinGame(sessionId: string)` | | Join game lobby |
| `LeaveGame(sessionId: string)` | | Leave lobby or forfeit in-game |
| `StartGame(sessionId: string)` | | Host only. Requires minPlayers met. |
| `SubmitAction(sessionId: string, action: GameActionDto)` | `{actionType, targetPlayerId?, data?}` | In-game action (vote, play card, answer, etc.) |

### Server → Client
| Event | Payload | Description |
|-------|---------|-------------|
| `gameCreated(session: GameSessionDto)` | `{id, gameType, hostId, maxPlayers, minPlayers}` | New game available in venue |
| `playerJoined(sessionId: string, player: GamePlayerDto)` | `{userId, displayName, avatar}` | Player joined lobby |
| `playerLeft(sessionId: string, userId: string)` | | Player left |
| `gameStarted(sessionId: string, playerState: object)` | Player-specific state (fog of war) | Game begins — each player gets only THEIR data |
| `stateUpdated(sessionId: string, state: object)` | Player-specific visible state | State changed after action |
| `phaseChanged(sessionId: string, phase: string, data: object)` | `{phase, timeoutSeconds, data}` | Night→Day→Vote transition (Mafia) or next turn |
| `playerEliminated(sessionId: string, userId: string, role?: string)` | | Player eliminated (Mafia) or finished (Uno) |
| `gameEnded(sessionId: string, result: GameResultDto)` | `{winnerId?, winnerTeam?, scores, duration}` | Game over |
| `playerDisconnected(sessionId: string, userId: string)` | | Player lost connection (60s grace) |
| `playerReconnected(sessionId: string, userId: string)` | | Player back |
| `turnTimeout(sessionId: string, userId: string)` | | Player didn't act in time — auto-action applied |

---

## PrivateChatHub — `/hubs/private-chat`
**Auth:** JWT Bearer required

### Client → Server
| Method | Params | Description |
|--------|--------|-------------|
| `SendPrivateMessage(conversationId: string, content: string, type: string, metadata?: object)` | | Send DM. Types: text, image, gif, voice, game_invite, gift |
| `MarkRead(conversationId: string, messageId: string)` | | Mark messages as read up to this ID |
| `StartTyping(conversationId: string)` | | Typing indicator |
| `StopTyping(conversationId: string)` | | Stop typing |

### Server → Client
| Event | Payload | Description |
|-------|---------|-------------|
| `receivePrivateMessage(msg: PrivateMessageDto)` | Same as ChatMessageDto + conversationId | New DM received |
| `typingStarted(conversationId: string, userId: string)` | | Other person typing |
| `typingStopped(conversationId: string, userId: string)` | | Stopped typing |
| `messagesRead(conversationId: string, readByUserId: string, upToMessageId: string)` | | Read receipt |
| `userOnlineStatusChanged(userId: string, isOnline: bool, lastSeenAt?: string)` | | Online/offline status |
| `giftReceived(gift: GiftEventDto)` | | Gift in private chat — only both see |

---

## NotificationHub — `/hubs/notifications`
**Auth:** JWT Bearer required. Auto-connect on app start.

### Server → Client (push-like real-time)
| Event | Payload | Description |
|-------|---------|-------------|
| `matchRequestReceived(request: MatchRequestDto)` | `{id, senderId, senderName, senderAvatar, introMessage, venueId}` | New match request |
| `matchAccepted(conversationId: string, userId: string, userName: string)` | | Match accepted — open chat |
| `vibeBombReceived(bomb: VibeBombDto)` | `{id, hint, guessesRemaining, expiresAt}` | Vibe Bomb! |
| `chainPartyInvite(chain: ChainPartyDto)` | `{id, venues[], reward}` | Chain Party invitation |
| `timeCapsuleUnlocked(capsule: TimeCapsuleDto)` | `{id, venueId, venueName, mediaUrls, message, createdAt}` | Capsule ready! |

---

## Connection Management
- **Auto-reconnect**: client uses exponential backoff (1s, 2s, 4s, 8s, max 30s)
- **Auth**: JWT sent as query param `?access_token={jwt}` on connection
- **Groups**: each venue chat = SignalR group `venue_{venueId}`, each conversation = `dm_{conversationId}`
- **Redis backplane**: all hubs use Redis for horizontal scaling
- **Keep-alive**: server ping every 15s, client timeout 30s
