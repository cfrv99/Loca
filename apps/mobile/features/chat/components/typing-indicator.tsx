import { View, Text, Animated } from 'react-native';
import { useEffect, useRef } from 'react';

interface Props {
  names: string[];
}

function AnimatedDot({ delay }: { delay: number }) {
  const opacity = useRef(new Animated.Value(0.3)).current;

  useEffect(() => {
    const animation = Animated.loop(
      Animated.sequence([
        Animated.timing(opacity, {
          toValue: 1,
          duration: 400,
          delay,
          useNativeDriver: true,
        }),
        Animated.timing(opacity, {
          toValue: 0.3,
          duration: 400,
          useNativeDriver: true,
        }),
      ]),
    );
    animation.start();
    return () => animation.stop();
  }, [delay, opacity]);

  return (
    <Animated.View
      style={{ opacity }}
      className="w-1.5 h-1.5 rounded-full bg-gray-500 dark:bg-gray-400 mx-0.5"
    />
  );
}

export function TypingIndicator({ names }: Props) {
  if (names.length === 0) return null;

  const displayText =
    names.length === 1
      ? `${names[0]} yazir...`
      : names.length === 2
        ? `${names[0]} və ${names[1]} yazir...`
        : `${names[0]} və ${names.length - 1} nəfər yazir...`;

  return (
    <View
      className="flex-row items-center px-4 py-1.5"
      accessibilityLabel={displayText}
      accessibilityRole="text"
    >
      <View className="flex-row items-center mr-2">
        <AnimatedDot delay={0} />
        <AnimatedDot delay={200} />
        <AnimatedDot delay={400} />
      </View>
      <Text className="text-xs text-gray-500 dark:text-gray-400 italic">
        {displayText}
      </Text>
    </View>
  );
}
