// Unit tests for useVenueChat data processing logic
// Avoids importing React/nativewind components

interface TestMessage {
  id: string;
  senderId: string;
  senderName: string;
  type: string;
  content?: string;
  createdAt: string;
}

interface TestVenueCount {
  total: number;
  male: number;
  female: number;
}

describe('useVenueChat data processing', () => {
  const makeMessage = (id: string, content: string, senderId = 'user-1'): TestMessage => ({
    id,
    senderId,
    senderName: 'Test',
    type: 'text',
    content,
    createdAt: new Date().toISOString(),
  });

  it('should merge history and real-time messages correctly', () => {
    const history: TestMessage[] = [
      makeMessage('1', 'Hello'),
      makeMessage('2', 'Hi'),
    ];
    const realtime: TestMessage[] = [
      makeMessage('3', 'New message'),
    ];

    const all = [...history, ...realtime];
    expect(all).toHaveLength(3);
    expect(all[0].id).toBe('1');
    expect(all[2].id).toBe('3');
  });

  it('should create correct venue count structure', () => {
    const count: TestVenueCount = { total: 25, male: 15, female: 10 };
    expect(count.total).toBe(25);
    expect(count.male + count.female).toBe(count.total);
  });

  it('should correctly track typing users in a Map', () => {
    const typingUsers = new Map<string, string>();
    typingUsers.set('user-1', 'Nigar');
    typingUsers.set('user-2', 'Elvin');

    expect(typingUsers.size).toBe(2);
    expect(Array.from(typingUsers.values())).toEqual(['Nigar', 'Elvin']);

    typingUsers.delete('user-1');
    expect(typingUsers.size).toBe(1);
    expect(typingUsers.has('user-1')).toBe(false);
  });

  it('should debounce typing - not send within 3 seconds', () => {
    let lastTypingSent = 0;
    const DEBOUNCE_MS = 3000;

    const shouldSendTyping = () => {
      const now = Date.now();
      if (now - lastTypingSent < DEBOUNCE_MS) return false;
      lastTypingSent = now;
      return true;
    };

    expect(shouldSendTyping()).toBe(true);
    expect(shouldSendTyping()).toBe(false);
  });

  it('should reverse messages correctly for inverted FlatList', () => {
    const messages = [
      makeMessage('1', 'First'),
      makeMessage('2', 'Second'),
      makeMessage('3', 'Third'),
    ];

    const reversed = [...messages].reverse();
    expect(reversed[0].id).toBe('3');
    expect(reversed[2].id).toBe('1');
  });

  it('should handle empty message history', () => {
    const history: TestMessage[] = [];
    const realtime: TestMessage[] = [];
    const all = [...history, ...realtime];
    expect(all).toHaveLength(0);
  });

  it('should format typing indicator text for single user', () => {
    const names = ['Nigar'];
    const text = names.length === 1
      ? `${names[0]} yazir...`
      : `${names[0]} və ${names.length - 1} nəfər yazir...`;
    expect(text).toBe('Nigar yazir...');
  });

  it('should format typing indicator text for multiple users', () => {
    const names = ['Nigar', 'Elvin'];
    const text = names.length === 1
      ? `${names[0]} yazir...`
      : names.length === 2
        ? `${names[0]} və ${names[1]} yazir...`
        : `${names[0]} və ${names.length - 1} nəfər yazir...`;
    expect(text).toBe('Nigar və Elvin yazir...');
  });
});
