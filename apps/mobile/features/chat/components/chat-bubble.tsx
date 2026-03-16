import { View, Text, Image, Pressable } from 'react-native';
import { useState } from 'react';
import type { ChatMessageDto } from '../../../shared/types';
import { ReactionPicker } from './reaction-picker';

interface Props {
  message: ChatMessageDto;
  isOwn: boolean;
  onReaction?: (messageId: string, emoji: string) => void;
}

function formatTime(isoDate: string): string {
  const date = new Date(isoDate);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}

export function ChatBubble({ message, isOwn, onReaction }: Props) {
  const [showReactions, setShowReactions] = useState(false);

  // System messages: centered, gray, italic
  if (message.type === 'system') {
    return (
      <View className="py-2 px-4" accessibilityLabel={`Sistem mesajı: ${message.content}`}>
        <Text className="text-xs text-gray-400 dark:text-gray-500 text-center italic">
          {message.content}
        </Text>
      </View>
    );
  }

  const handleLongPress = () => {
    setShowReactions(true);
  };

  const handleReaction = (emoji: string) => {
    setShowReactions(false);
    onReaction?.(message.id, emoji);
  };

  return (
    <View className={`flex-row mb-2 ${isOwn ? 'justify-end' : 'justify-start'}`}>
      {/* Avatar (other user only) */}
      {!isOwn && (
        <View className="w-8 h-8 rounded-full bg-gray-200 dark:bg-gray-600 items-center justify-center mr-2 mt-1">
          {message.senderAvatar ? (
            <Image
              source={{ uri: message.senderAvatar }}
              className="w-8 h-8 rounded-full"
              accessibilityLabel={`${message.senderName} avatar`}
            />
          ) : (
            <Text className="text-xs">{message.senderName.charAt(0).toUpperCase()}</Text>
          )}
        </View>
      )}

      <View className={`max-w-[75%] ${isOwn ? 'items-end' : 'items-start'}`}>
        {/* Sender name (other user only) */}
        {!isOwn && (
          <Text className="text-xs text-gray-500 dark:text-gray-400 mb-1 ml-1">
            {message.senderName}
          </Text>
        )}

        {/* Reply preview */}
        {message.replyTo && (
          <View
            className={`rounded-t-xl px-3 py-1.5 mb-0.5 ${
              isOwn ? 'bg-blue-400 dark:bg-blue-700' : 'bg-gray-200 dark:bg-gray-600'
            }`}
            accessibilityLabel={`Cavab: ${message.replyTo.senderName}: ${message.replyTo.content ?? ''}`}
          >
            <Text className="text-xs font-semibold text-gray-700 dark:text-gray-300" numberOfLines={1}>
              {message.replyTo.senderName}
            </Text>
            <Text className="text-xs text-gray-600 dark:text-gray-400" numberOfLines={1}>
              {message.replyTo.content ?? ''}
            </Text>
          </View>
        )}

        <Pressable
          onLongPress={handleLongPress}
          className={`rounded-2xl px-4 py-2.5 ${
            isOwn
              ? 'bg-accent rounded-tr-sm'
              : 'bg-gray-100 dark:bg-gray-700 rounded-tl-sm'
          }`}
          accessibilityLabel={`${message.senderName} mesajı: ${message.content ?? ''}`}
          accessibilityRole="text"
        >
          {/* Image message */}
          {message.type === 'image' && message.mediaUrl && (
            <Image
              source={{ uri: message.mediaUrl }}
              className="w-52 h-40 rounded-xl mb-1"
              resizeMode="cover"
              accessibilityLabel="Paylasilmis sekil"
            />
          )}

          {/* GIF message */}
          {message.type === 'gif' && message.mediaUrl && (
            <Image
              source={{ uri: message.mediaUrl }}
              className="w-48 h-36 rounded-xl mb-1"
              resizeMode="cover"
              accessibilityLabel="GIF"
            />
          )}

          {/* Voice message */}
          {message.type === 'voice' && (
            <View className="flex-row items-center gap-2">
              <View className="w-6 h-6 rounded-full bg-white/30 items-center justify-center">
                <Text className="text-xs">▶</Text>
              </View>
              <View className="flex-row items-center gap-0.5">
                {Array.from({ length: 20 }).map((_, i) => (
                  <View
                    key={i}
                    className={`w-1 rounded-full ${isOwn ? 'bg-white/60' : 'bg-gray-400'}`}
                    style={{ height: Math.random() * 16 + 4 }}
                  />
                ))}
              </View>
              <Text className={`text-xs ${isOwn ? 'text-white/70' : 'text-gray-500'}`}>
                0:30
              </Text>
            </View>
          )}

          {/* Text content */}
          {message.content && (message.type === 'text' || message.type === 'emoji') && (
            <Text
              className={`text-base ${
                isOwn ? 'text-white' : 'text-primary dark:text-white'
              }`}
            >
              {message.content}
            </Text>
          )}
        </Pressable>

        {/* Timestamp */}
        <Text className="text-xs text-gray-400 dark:text-gray-500 mt-0.5 mx-1">
          {formatTime(message.createdAt)}
        </Text>

        {/* Reaction picker popup */}
        {showReactions && (
          <ReactionPicker
            onSelect={handleReaction}
            onClose={() => setShowReactions(false)}
          />
        )}
      </View>
    </View>
  );
}
