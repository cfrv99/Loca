export const API_BASE_URL = __DEV__
  ? 'http://localhost:5001/api/v1'
  : 'https://api.loca.az/api/v1';

export const SIGNALR_BASE_URL = __DEV__
  ? 'http://localhost:5001'
  : 'https://api.loca.az';

export const APP_CONFIG = {
  maxChatMessageLength: 2000,
  qrRotationSeconds: 60,
  geofenceDefaultRadius: 150,
  matchRequestDailyLimit: 5,
  chatRateLimit: 30, // messages per minute
  reconnectMaxRetries: 10,
  reconnectBaseDelay: 1000,
} as const;
