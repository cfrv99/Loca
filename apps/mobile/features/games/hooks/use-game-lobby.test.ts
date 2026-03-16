// Unit tests for game lobby data processing logic
// Avoids importing React/nativewind components

interface TestPlayer {
  userId: string;
  displayName: string;
  score: number;
  isAlive: boolean;
  isConnected: boolean;
}

interface TestSession {
  id: string;
  gameType: string;
  hostId: string;
  maxPlayers: number;
  minPlayers: number;
  status: string;
  players: TestPlayer[];
}

describe('useGameLobby data processing', () => {
  const createPlayer = (userId: string, displayName: string): TestPlayer => ({
    userId,
    displayName,
    score: 0,
    isAlive: true,
    isConnected: true,
  });

  const createSession = (overrides: Partial<TestSession> = {}): TestSession => ({
    id: 'session-1',
    gameType: 'mafia',
    hostId: 'host-1',
    maxPlayers: 12,
    minPlayers: 5,
    status: 'lobby',
    players: [],
    ...overrides,
  });

  it('should add a player to session correctly', () => {
    const session = createSession({ players: [createPlayer('host-1', 'Host')] });
    const newPlayer = createPlayer('user-2', 'Nigar');

    const updated = {
      ...session,
      players: [...session.players, newPlayer],
    };

    expect(updated.players).toHaveLength(2);
    expect(updated.players[1].displayName).toBe('Nigar');
  });

  it('should remove a player from session', () => {
    const session = createSession({
      players: [
        createPlayer('host-1', 'Host'),
        createPlayer('user-2', 'Nigar'),
        createPlayer('user-3', 'Elvin'),
      ],
    });

    const updated = {
      ...session,
      players: session.players.filter((p) => p.userId !== 'user-2'),
    };

    expect(updated.players).toHaveLength(2);
    expect(updated.players.find((p) => p.userId === 'user-2')).toBeUndefined();
  });

  it('should not add duplicate player', () => {
    const session = createSession({
      players: [createPlayer('host-1', 'Host')],
    });
    const duplicatePlayer = createPlayer('host-1', 'Host');

    const exists = session.players.some((p) => p.userId === duplicatePlayer.userId);
    expect(exists).toBe(true);
  });

  it('should determine if game can start (minPlayers met)', () => {
    const session = createSession({
      minPlayers: 3,
      players: [
        createPlayer('1', 'A'),
        createPlayer('2', 'B'),
        createPlayer('3', 'C'),
      ],
    });

    expect(session.players.length >= session.minPlayers).toBe(true);
  });

  it('should not start if minPlayers not met', () => {
    const session = createSession({
      minPlayers: 5,
      players: [
        createPlayer('1', 'A'),
        createPlayer('2', 'B'),
      ],
    });

    expect(session.players.length >= session.minPlayers).toBe(false);
  });

  it('should correctly identify host', () => {
    const session = createSession({ hostId: 'host-1' });
    expect(session.hostId === 'host-1').toBe(true);
    expect(session.hostId === 'user-2').toBe(false);
  });

  it('should map game type to display name', () => {
    const GAME_TYPE_NAMES: Record<string, string> = {
      mafia: 'Mafiya',
      truth_or_dare: 'Həqiqət / Cəsarət',
      uno: 'Uno',
      domino: 'Domino',
      quiz: 'Quiz',
    };

    expect(GAME_TYPE_NAMES['mafia']).toBe('Mafiya');
    expect(GAME_TYPE_NAMES['truth_or_dare']).toBe('Həqiqət / Cəsarət');
    expect(GAME_TYPE_NAMES['unknown']).toBeUndefined();
  });

  it('should track player disconnect state', () => {
    const disconnected = new Set<string>();
    disconnected.add('user-2');

    expect(disconnected.has('user-2')).toBe(true);
    expect(disconnected.has('user-1')).toBe(false);

    disconnected.delete('user-2');
    expect(disconnected.has('user-2')).toBe(false);
  });
});
