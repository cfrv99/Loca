const DEV_API_URL = 'http://localhost:5000';
const PROD_API_URL = 'https://api.loca.az';

export const CONFIG = {
  API_URL: __DEV__ ? DEV_API_URL : PROD_API_URL,
  SIGNALR_CHAT_HUB: '/hubs/venue-chat',
  SIGNALR_GAME_HUB: '/hubs/game',
  SIGNALR_PRIVATE_CHAT_HUB: '/hubs/private-chat',
  SIGNALR_NOTIFICATION_HUB: '/hubs/notifications',
  DEFAULT_PAGE_SIZE: 20,
  GEOFENCE_DEFAULT_RADIUS: 150,
  QR_ROTATION_SECONDS: 60,
  MATCH_DAILY_LIMIT: 5,
  MESSAGE_MAX_LENGTH: 1000,
};
