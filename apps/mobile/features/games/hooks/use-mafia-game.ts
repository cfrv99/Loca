import { useState, useEffect, useCallback } from 'react';
import { useSignalR } from '../../../shared/hooks/use-signalr';
import { CONFIG } from '../../../shared/constants/config';
import type {
  MafiaPlayerState,
  MafiaPhaseData,
  GameResultDto,
} from '../../../shared/types';

export function useMafiaGame(sessionId: string) {
  const { connection, isConnected } = useSignalR(CONFIG.SIGNALR_GAME_HUB);
  const [myRole, setMyRole] = useState<MafiaPlayerState | null>(null);
  const [phase, setPhase] = useState<MafiaPhaseData | null>(null);
  const [result, setResult] = useState<GameResultDto | null>(null);
  const [eliminatedPlayer, setEliminatedPlayer] = useState<{
    userId: string;
    displayName: string;
    role?: string;
  } | null>(null);
  const [disconnectedPlayers, setDisconnectedPlayers] = useState<Set<string>>(new Set());
  const [timeRemaining, setTimeRemaining] = useState(0);

  useEffect(() => {
    if (!connection || !isConnected) return;

    const onGameStarted = (sid: string, playerState: MafiaPlayerState) => {
      if (sid !== sessionId) return;
      setMyRole(playerState);
    };

    const onStateUpdated = (sid: string, state: { playerState: MafiaPlayerState }) => {
      if (sid !== sessionId) return;
      setMyRole(state.playerState);
    };

    const onPhaseChanged = (sid: string, phaseName: string, data: MafiaPhaseData) => {
      if (sid !== sessionId) return;
      setPhase({ ...data, phase: phaseName as MafiaPhaseData['phase'] });
      setTimeRemaining(data.timeoutSeconds);
      setEliminatedPlayer(null);
    };

    const onPlayerEliminated = (sid: string, userId: string, role?: string) => {
      if (sid !== sessionId) return;
      const player = phase?.alivePlayers.find((p) => p.userId === userId);
      setEliminatedPlayer({
        userId,
        displayName: player?.displayName ?? 'Oyuncu',
        role,
      });
    };

    const onGameEnded = (sid: string, gameResult: GameResultDto) => {
      if (sid !== sessionId) return;
      setResult(gameResult);
    };

    const onPlayerDisconnected = (sid: string, userId: string) => {
      if (sid !== sessionId) return;
      setDisconnectedPlayers((prev) => new Set(prev).add(userId));
    };

    const onPlayerReconnected = (sid: string, userId: string) => {
      if (sid !== sessionId) return;
      setDisconnectedPlayers((prev) => {
        const next = new Set(prev);
        next.delete(userId);
        return next;
      });
    };

    connection.on('gameStarted', onGameStarted);
    connection.on('stateUpdated', onStateUpdated);
    connection.on('phaseChanged', onPhaseChanged);
    connection.on('playerEliminated', onPlayerEliminated);
    connection.on('gameEnded', onGameEnded);
    connection.on('playerDisconnected', onPlayerDisconnected);
    connection.on('playerReconnected', onPlayerReconnected);

    return () => {
      connection.off('gameStarted', onGameStarted);
      connection.off('stateUpdated', onStateUpdated);
      connection.off('phaseChanged', onPhaseChanged);
      connection.off('playerEliminated', onPlayerEliminated);
      connection.off('gameEnded', onGameEnded);
      connection.off('playerDisconnected', onPlayerDisconnected);
      connection.off('playerReconnected', onPlayerReconnected);
    };
  }, [connection, isConnected, sessionId, phase?.alivePlayers]);

  // Countdown timer
  useEffect(() => {
    if (timeRemaining <= 0) return;
    const timer = setInterval(() => {
      setTimeRemaining((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => clearInterval(timer);
  }, [timeRemaining]);

  const submitVote = useCallback(
    async (targetPlayerId: string) => {
      if (!connection || !isConnected) return;
      await connection.invoke('SubmitAction', sessionId, {
        actionType: 'vote',
        targetPlayerId,
      });
    },
    [connection, isConnected, sessionId],
  );

  const submitNightAction = useCallback(
    async (targetPlayerId: string) => {
      if (!connection || !isConnected) return;
      const actionType = myRole?.role === 'mafia' ? 'kill'
        : myRole?.role === 'doctor' ? 'save'
        : myRole?.role === 'detective' ? 'investigate'
        : 'none';
      await connection.invoke('SubmitAction', sessionId, {
        actionType,
        targetPlayerId,
      });
    },
    [connection, isConnected, sessionId, myRole?.role],
  );

  return {
    myRole,
    phase,
    result,
    eliminatedPlayer,
    disconnectedPlayers,
    timeRemaining,
    isConnected,
    submitVote,
    submitNightAction,
  };
}
