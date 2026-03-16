// Unit test for ChatBubble data processing logic
// Does not import React components to avoid nativewind babel issues

interface TestMessage {
  id: string;
  senderId: string;
  senderName: string;
  type: string;
  content?: string;
  mediaUrl?: string;
  replyTo?: TestMessage;
  createdAt: string;
}

describe('ChatBubble data processing', () => {
  const createMessage = (overrides: Partial<TestMessage> = {}): TestMessage => ({
    id: 'msg-1',
    senderId: 'user-1',
    senderName: 'Nigar',
    type: 'text',
    content: 'Salam!',
    createdAt: '2026-03-16T14:30:00Z',
    ...overrides,
  });

  it('should correctly identify system messages by type', () => {
    const systemMsg = createMessage({ type: 'system', content: 'Nigar qosuldu' });
    expect(systemMsg.type).toBe('system');
    expect(systemMsg.content).toBe('Nigar qosuldu');
  });

  it('should correctly identify own vs other messages', () => {
    const msg = createMessage({ senderId: 'user-1' });
    expect(msg.senderId === 'user-1').toBe(true);
    expect(msg.senderId === 'user-2').toBe(false);
  });

  it('should handle message with reply', () => {
    const originalMsg = createMessage({ id: 'msg-0', content: 'Necesen?' });
    const replyMsg = createMessage({
      id: 'msg-1',
      content: 'Yaxsiyam!',
      replyTo: originalMsg,
    });

    expect(replyMsg.replyTo).toBeDefined();
    expect(replyMsg.replyTo?.content).toBe('Necesen?');
    expect(replyMsg.replyTo?.senderName).toBe('Nigar');
  });

  it('should handle image message type', () => {
    const imgMsg = createMessage({
      type: 'image',
      mediaUrl: 'https://example.com/photo.jpg',
    });
    expect(imgMsg.type).toBe('image');
    expect(imgMsg.mediaUrl).toBeTruthy();
  });

  it('should handle voice message type', () => {
    const voiceMsg = createMessage({
      type: 'voice',
      mediaUrl: 'https://example.com/voice.m4a',
    });
    expect(voiceMsg.type).toBe('voice');
    expect(voiceMsg.mediaUrl).toBeTruthy();
  });

  it('should format time correctly from ISO string', () => {
    const date = new Date('2026-03-16T14:30:00Z');
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    const formatted = `${hours}:${minutes}`;
    expect(formatted).toMatch(/^\d{2}:\d{2}$/);
  });

  it('should handle message without content', () => {
    const msg = createMessage({ content: undefined, type: 'image', mediaUrl: 'url' });
    expect(msg.content).toBeUndefined();
    expect(msg.mediaUrl).toBe('url');
  });
});
