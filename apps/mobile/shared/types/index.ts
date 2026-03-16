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
