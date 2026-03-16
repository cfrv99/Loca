import { useState, useEffect, useCallback } from 'react';
import { useSignalR } from '../../../shared/hooks/use-signalr';
import { CONFIG } from '../../../shared/constants/config';
import type {
  GameSessionDto,
  GamePlayerDto,
  GameResultDto,
  GameStateDto,
} from '../../../shared/types';

export function useGameLobby(sessionId?: string) {
  const { connection, isConnected } = useSignalR(CONFIG.SIGNALR_GAME_HUB);
  const [session, setSession] = useState<GameSessionDto | null>(null);
  const [gameStarted, setGameStarted] = useState(false);
  const [playerState, setPlayerState] = useState<GameStateDto | null>(null);
  const [gameResult, setGameResult] = useState<GameResultDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!connection || !isConnected) return;

    const onPlayerJoined = (sid: string, player: GamePlayerDto) => {
      if (sid !== sessionId) return;
      setSession((prev) => {
        if (!prev) return prev;
        const exists = prev.players.some((p) => p.userId === player.userId);
        if (exists) return prev;
        return { ...prev, players: [...prev.players, player] };
      });
    };

    const onPlayerLeft = (sid: string, userId: string) => {
      if (sid !== sessionId) return;
      setSession((prev) => {
        if (!prev) return prev;
        return { ...prev, players: prev.players.filter((p) => p.userId !== userId) };
      });
    };

    const onGameStarted = (sid: string, state: GameStateDto) => {
      if (sid !== sessionId) return;
      setGameStarted(true);
      setPlayerState(state);
    };

    const onStateUpdated = (sid: string, state: GameStateDto) => {
      if (sid !== sessionId) return;
      setPlayerState(state);
    };

    const onGameEnded = (sid: string, result: GameResultDto) => {
      if (sid !== sessionId) return;
      setGameResult(result);
    };

    connection.on('playerJoined', onPlayerJoined);
    connection.on('playerLeft', onPlayerLeft);
    connection.on('gameStarted', onGameStarted);
    connection.on('stateUpdated', onStateUpdated);
    connection.on('gameEnded', onGameEnded);

    return () => {
      connection.off('playerJoined', onPlayerJoined);
      connection.off('playerLeft', onPlayerLeft);
      connection.off('gameStarted', onGameStarted);
      connection.off('stateUpdated', onStateUpdated);
      connection.off('gameEnded', onGameEnded);
    };
  }, [connection, isConnected, sessionId]);

  const createGame = useCallback(
    async (venueId: string, gameType: string, maxPlayers: number, settings?: Record<string, unknown>) => {
      if (!connection || !isConnected) return null;
      try {
        const result = await connection.invoke<GameSessionDto>(
          'CreateGame', venueId, gameType, maxPlayers, settings ?? null,
        );
        setSession(result);
        return result;
      } catch (err) {
        setError('Oyun yaradilmadi');
        return null;
      }
    },
    [connection, isConnected],
  );

  const joinGame = useCallback(
    async (sid: string) => {
      if (!connection || !isConnected) return;
      try {
        await connection.invoke('JoinGame', sid);
      } catch {
        setError('Oyuna qosulmaq mumkun olmadi');
      }
    },
    [connection, isConnected],
  );

  const leaveGame = useCallback(
    async (sid: string) => {
      if (!connection || !isConnected) return;
      await connection.invoke('LeaveGame', sid).catch(() => {});
    },
    [connection, isConnected],
  );

  const startGame = useCallback(
    async (sid: string) => {
      if (!connection || !isConnected) return;
      try {
        await connection.invoke('StartGame', sid);
      } catch {
        setError('Oyun baslaya bilmedi');
      }
    },
    [connection, isConnected],
  );

  const submitAction = useCallback(
    async (sid: string, actionType: string, targetPlayerId?: string, data?: Record<string, unknown>) => {
      if (!connection || !isConnected) return;
      await connection.invoke('SubmitAction', sid, { actionType, targetPlayerId, data });
    },
    [connection, isConnected],
  );

  return {
    session,
    setSession,
    gameStarted,
    playerState,
    gameResult,
    error,
    isConnected,
    createGame,
    joinGame,
    leaveGame,
    startGame,
    submitAction,
  };
}
