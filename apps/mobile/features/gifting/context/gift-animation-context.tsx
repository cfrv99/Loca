import { createContext, useContext, useState, useCallback, useEffect, useRef } from 'react';
import { View, Text, Animated, Dimensions, StyleSheet } from 'react-native';
import type { GiftEventDto } from '../../../shared/types';

type ReactNode = React.ReactNode;

interface GiftAnimationContextType {
  triggerGift: (gift: GiftEventDto) => void;
}

const GiftAnimationContext = createContext<GiftAnimationContextType>({
  triggerGift: () => {},
});

export function useGiftAnimation() {
  return useContext(GiftAnimationContext);
}

const { width: SCREEN_WIDTH, height: SCREEN_HEIGHT } = Dimensions.get('window');

const TIER_CONFIG = {
  basic: { particleCount: 8, duration: 2500, maxScale: 1.2 },
  premium: { particleCount: 16, duration: 3000, maxScale: 1.5 },
  luxury: { particleCount: 24, duration: 4000, maxScale: 1.8 },
};

const GIFT_ICONS: Record<string, string> = {
  rose: '🌹',
  heart: '❤️',
  like: '👍',
  party: '🎉',
  cocktail: '🍸',
  pizza: '🍕',
  firework: '🎆',
  crown: '👑',
  car: '🏎️',
  villa: '🏠',
  ring: '💍',
  helicopter: '🚁',
};

function GiftOverlay({ gift, onDone }: { gift: GiftEventDto; onDone: () => void }) {
  const config = TIER_CONFIG[gift.giftTier] ?? TIER_CONFIG.basic;
  const scaleAnim = useRef(new Animated.Value(0)).current;
  const opacityAnim = useRef(new Animated.Value(0)).current;
  const particles = useRef(
    Array.from({ length: config.particleCount }).map(() => ({
      x: new Animated.Value(SCREEN_WIDTH / 2),
      y: new Animated.Value(SCREEN_HEIGHT / 2),
      opacity: new Animated.Value(1),
      scale: new Animated.Value(0.5),
    })),
  ).current;

  useEffect(() => {
    // Main icon animation
    Animated.sequence([
      Animated.parallel([
        Animated.spring(scaleAnim, {
          toValue: config.maxScale,
          friction: 4,
          tension: 60,
          useNativeDriver: true,
        }),
        Animated.timing(opacityAnim, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true,
        }),
      ]),
      Animated.delay(config.duration - 800),
      Animated.parallel([
        Animated.timing(scaleAnim, {
          toValue: 0,
          duration: 500,
          useNativeDriver: true,
        }),
        Animated.timing(opacityAnim, {
          toValue: 0,
          duration: 500,
          useNativeDriver: true,
        }),
      ]),
    ]).start(() => onDone());

    // Particle animations
    particles.forEach((p) => {
      const angle = Math.random() * Math.PI * 2;
      const distance = 100 + Math.random() * 150;
      const targetX = SCREEN_WIDTH / 2 + Math.cos(angle) * distance;
      const targetY = SCREEN_HEIGHT / 2 + Math.sin(angle) * distance - 100;

      Animated.parallel([
        Animated.timing(p.x, {
          toValue: targetX,
          duration: config.duration * 0.7,
          useNativeDriver: true,
        }),
        Animated.timing(p.y, {
          toValue: targetY - 200,
          duration: config.duration * 0.7,
          useNativeDriver: true,
        }),
        Animated.sequence([
          Animated.timing(p.scale, {
            toValue: 1,
            duration: 300,
            useNativeDriver: true,
          }),
          Animated.delay(config.duration * 0.4),
          Animated.timing(p.opacity, {
            toValue: 0,
            duration: config.duration * 0.3,
            useNativeDriver: true,
          }),
        ]),
      ]).start();
    });
  }, [config.duration, config.maxScale, opacityAnim, particles, scaleAnim]);

  const giftIcon = GIFT_ICONS[gift.giftName.toLowerCase()] ?? '🎁';

  return (
    <View style={StyleSheet.absoluteFill} pointerEvents="none">
      {/* Semi-transparent overlay */}
      <View
        style={StyleSheet.absoluteFill}
        className="bg-black/30"
      />

      {/* Particles */}
      {particles.map((p, i) => (
        <Animated.View
          key={i}
          style={{
            position: 'absolute',
            transform: [
              { translateX: Animated.subtract(p.x, SCREEN_WIDTH / 2) },
              { translateY: Animated.subtract(p.y, SCREEN_HEIGHT / 2) },
              { scale: p.scale },
            ],
            opacity: p.opacity,
            left: SCREEN_WIDTH / 2 - 6,
            top: SCREEN_HEIGHT / 2 - 6,
          }}
        >
          <View
            className={`w-3 h-3 rounded-full ${
              i % 3 === 0 ? 'bg-warning' : i % 3 === 1 ? 'bg-accent' : 'bg-secondary'
            }`}
          />
        </Animated.View>
      ))}

      {/* Main gift icon */}
      <Animated.View
        style={{
          position: 'absolute',
          left: SCREEN_WIDTH / 2 - 40,
          top: SCREEN_HEIGHT / 2 - 80,
          transform: [{ scale: scaleAnim }],
          opacity: opacityAnim,
          alignItems: 'center',
        }}
      >
        <Text style={{ fontSize: 64 }}>{giftIcon}</Text>
        <Text className="text-white text-lg font-bold mt-2 text-center">
          {gift.giftNameAz ?? gift.giftName}
        </Text>
        <Text className="text-white/80 text-sm mt-1">
          {gift.senderName} gondərdi
        </Text>
      </Animated.View>
    </View>
  );
}

export function GiftAnimationProvider({ children }: { children: ReactNode }) {
  const [queue, setQueue] = useState<GiftEventDto[]>([]);
  const [current, setCurrent] = useState<GiftEventDto | null>(null);

  const triggerGift = useCallback((gift: GiftEventDto) => {
    setQueue((prev) => [...prev, gift]);
  }, []);

  // Process queue
  useEffect(() => {
    if (current === null && queue.length > 0) {
      setCurrent(queue[0]);
      setQueue((prev) => prev.slice(1));
    }
  }, [current, queue]);

  const handleDone = useCallback(() => {
    setCurrent(null);
  }, []);

  return (
    <GiftAnimationContext.Provider value={{ triggerGift }}>
      {children}
      {current && <GiftOverlay gift={current} onDone={handleDone} />}
    </GiftAnimationContext.Provider>
  );
}
