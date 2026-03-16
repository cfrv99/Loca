// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: { code: string; message: string };
}

export interface CursorPageResponse<T> {
  items: T[];
  nextCursor?: string;
  hasMore: boolean;
}

// Auth
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
  isNewUser: boolean;
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  dateOfBirth: string;
  gender: string;
  interests: string[];
  purposes: string[];
  vibePreferences: VibePreferenceDto[];
  isOnboarded: boolean;
  isPremium: boolean;
  coinBalance: number;
  createdAt: string;
}

export interface VibePreferenceDto {
  vibe: string;
  weight: number;
}

export interface PublicUserDto {
  id: string;
  displayName: string;
  avatarUrl?: string;
  age: number;
  gender: string;
  interests: string[];
  purposes: string[];
  pastVenues: PastVenueDto[];
  memoriesCount: number;
  giftBadge?: string;
}

export interface PastVenueDto {
  venueId: string;
  venueName: string;
  lastVisit: string;
}

// Venue
export interface VenueCardDto {
  id: string;
  name: string;
  coverPhotoUrl?: string;
  address: string;
  category: string;
  distanceMeters: number;
  stats: VenueStatsDto;
  activityLevel: 'low' | 'medium' | 'high';
  activeGames: number;
  chatMessageCount: number;
}

export interface VenueStatsDto {
  total: number;
  male: number;
  female: number;
}

export interface VenueDetailDto {
  id: string;
  name: string;
  description?: string;
  address: string;
  category: string;
  latitude: number;
  longitude: number;
  coverPhotoUrl?: string;
  photoUrls: string[];
  googleRating?: number;
  phone?: string;
  website?: string;
  workingHours?: string;
  stats: VenueStatsDto;
  geofenceRadius: number;
}

export interface CheckInResultDto {
  checkInId: string;
  venueId: string;
  venueName: string;
  isAnonymous: boolean;
  checkedInAt: string;
}

// Chat
export interface ChatMessageDto {
  id: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  type: string;
  content?: string;
  mediaUrl?: string;
  replyTo?: ChatMessageDto;
  metadata?: Record<string, unknown>;
  createdAt: string;
}

// Game
export interface GameSessionDto {
  id: string;
  gameType: string;
  hostId: string;
  maxPlayers: number;
  minPlayers: number;
  status: string;
  players: GamePlayerDto[];
}

export interface GamePlayerDto {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  score: number;
  isAlive: boolean;
  isConnected: boolean;
}

// Economy
export interface GiftDto {
  id: string;
  name: string;
  nameAz?: string;
  tier: string;
  coinPrice: number;
  iconUrl?: string;
  animationUrl?: string;
  venueId?: string;
}

// Match
export interface MatchRequestDto {
  id: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  introMessage?: string;
  venueId: string;
  status: string;
  createdAt: string;
}

export interface ConversationDto {
  conversationId: string;
  otherUser: PublicUserDto;
  lastMessage?: ChatMessageDto;
  unreadCount: number;
  updatedAt: string;
}

export interface ActiveUserDto {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  age: number;
  interests: string[];
  isAnonymous: boolean;
}

// Onboarding
export interface OnboardingData {
  displayName: string;
  gender: string;
  avatarUri?: string;
  interests: string[];
  purposes: string[];
  vibePreferences: VibePreferenceDto[];
  privacySettings: PrivacySettingsDto;
}

export interface PrivacySettingsDto {
  defaultAnonymous: boolean;
  pushEnabled: boolean;
}

// Coin packages
export interface CoinPackageDto {
  id: string;
  name: string;
  nameAz?: string;
  priceAzn: number;
  coins: number;
  bonusCoins: number;
  iosProductId?: string;
  androidProductId?: string;
}

// Balance
export interface BalanceDto {
  coinBalance: number;
  totalPurchased: number;
  totalSpent: number;
}

// Post/Feed
export interface PostDto {
  id: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  content?: string;
  mediaUrls: string[];
  likeCount: number;
  commentCount: number;
  isLikedByMe: boolean;
  createdAt: string;
}

export interface CommentDto {
  id: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  content: string;
  createdAt: string;
}

// Report
export type ReportReason = 'harassment' | 'spam' | 'fake' | 'inappropriate' | 'other';

export interface ReportRequest {
  reason: ReportReason;
  description?: string;
}

// Gift send
export interface GiftSendRequest {
  giftId: string;
  recipientId: string;
  context: 'public_chat' | 'private_chat';
  venueId?: string;
  conversationId?: string;
}

export interface GiftSendResult {
  transactionId: string;
  newBalance: number;
}

// Reactions
export interface ReactionDto {
  emoji: string;
  count: number;
  userIds: string[];
}

// Gift Events (SignalR)
export interface GiftEventDto {
  senderId: string;
  senderName: string;
  recipientId?: string;
  recipientName?: string;
  giftId: string;
  giftName: string;
  giftNameAz?: string;
  giftTier: 'basic' | 'premium' | 'luxury';
  animationUrl?: string;
  iconUrl?: string;
  coinCost: number;
}

// Private Chat
export interface PrivateMessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  type: string;
  content?: string;
  mediaUrl?: string;
  replyTo?: PrivateMessageDto;
  metadata?: Record<string, unknown>;
  isRead: boolean;
  createdAt: string;
}

// Game types
export interface GameActionDto {
  actionType: string;
  targetPlayerId?: string;
  data?: Record<string, unknown>;
}

export interface GameResultDto {
  winnerId?: string;
  winnerTeam?: string;
  scores: Array<{ userId: string; displayName: string; score: number }>;
  duration: string;
}

export interface GameStateDto {
  phase: string;
  timeoutSeconds: number;
  players: GamePlayerDto[];
  currentTurnUserId?: string;
  data?: Record<string, unknown>;
}

// Mafia specific
export interface MafiaPlayerState {
  role: 'mafia' | 'doctor' | 'detective' | 'citizen';
  isAlive: boolean;
  mafiaTeamIds?: string[];
  investigationResult?: boolean;
}

export interface MafiaPhaseData {
  phase: 'night' | 'day_discussion' | 'day_vote' | 'elimination' | 'role_reveal' | 'game_over';
  timeoutSeconds: number;
  alivePlayers: Array<{ userId: string; displayName: string; avatarUrl?: string }>;
  eliminatedPlayer?: { userId: string; displayName: string; role?: string };
  votes?: Record<string, string>;
  winners?: 'mafia' | 'citizens';
}

// Truth or Dare specific
export interface TruthDareState {
  currentPlayerId: string;
  currentPlayerName: string;
  phase: 'spinning' | 'choosing' | 'showing' | 'waiting';
  questionType?: 'truth' | 'dare';
  questionText?: string;
  questionCategory?: string;
  timeoutSeconds: number;
}

// Venue count (SignalR)
export interface VenueCountDto {
  total: number;
  male: number;
  female: number;
}
