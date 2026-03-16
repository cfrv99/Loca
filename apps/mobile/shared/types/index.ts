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
  totalCount: number;
}

// Auth
export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
  isNewUser: boolean;
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  profilePhotoUrl?: string;
  bio?: string;
  isOnboardingComplete: boolean;
  createdAt: string;
}

export interface UserProfileDto {
  id: string;
  displayName: string;
  profilePhotoUrl?: string;
  bio?: string;
  interests: string[];
  totalCheckIns: number;
  totalGamesPlayed: number;
  totalGiftsReceived: number;
  totalMatchesMade: number;
}

// Venue
export interface VenueCardDto {
  id: string;
  name: string;
  address: string;
  category: string;
  coverPhotoUrl?: string;
  latitude: number;
  longitude: number;
  distanceMeters: number;
  activeCount: number;
  maleCount: number;
  femaleCount: number;
  activityLevel: 'low' | 'medium' | 'high';
}

export interface VenueDetailDto {
  id: string;
  name: string;
  description: string;
  address: string;
  category: string;
  coverPhotoUrl?: string;
  photoUrls: string[];
  phone?: string;
  website?: string;
  instagramHandle?: string;
  latitude: number;
  longitude: number;
  geofenceRadiusMeters: number;
  openingHours?: string;
  activeCount: number;
  isVerified: boolean;
  createdAt: string;
}

export interface CheckInResult {
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
  senderPhotoUrl?: string;
  type: string;
  content: string;
  mediaUrl?: string;
  sentAt: string;
}

// Game
export interface GameSessionDto {
  id: string;
  gameType: string;
  status: string;
  hostId: string;
  hostName: string;
  playerCount: number;
  minPlayers: number;
  maxPlayers: number;
  createdAt: string;
}

export interface GamePlayerDto {
  userId: string;
  displayName: string;
  profilePhotoUrl?: string;
  status: string;
  score: number;
}

// Economy
export interface WalletDto {
  id: string;
  balance: number;
  totalEarned: number;
  totalSpent: number;
}

export interface GiftCatalogItem {
  id: string;
  name: string;
  description: string;
  coinPrice: number;
  tier: string;
  animationUrl: string;
  iconUrl: string;
  sortOrder: number;
}

// Match
export interface MatchRequestDto {
  id: string;
  senderId: string;
  senderName: string;
  senderPhotoUrl?: string;
  venueId: string;
  venueName: string;
  introMessage?: string;
  status: 'pending' | 'accepted' | 'declined';
  createdAt: string;
}
