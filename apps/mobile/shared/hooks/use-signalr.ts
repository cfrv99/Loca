import { useEffect, useRef, useCallback, useState } from 'react';
import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import * as SecureStore from 'expo-secure-store';
import { SIGNALR_BASE_URL, APP_CONFIG } from '../constants/config';

interface UseSignalROptions {
  hubPath: string;
  enabled?: boolean;
}

export function useSignalR({ hubPath, enabled = true }: UseSignalROptions) {
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const retryCountRef = useRef(0);

  const connect = useCallback(async () => {
    if (connectionRef.current?.state === HubConnectionState.Connected) return;

    try {
      const token = await SecureStore.getItemAsync('access_token');
      if (!token) return;

      const connection = new HubConnectionBuilder()
        .withUrl(`${SIGNALR_BASE_URL}${hubPath}`, {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount >= APP_CONFIG.reconnectMaxRetries) {
              return null; // Stop retrying
            }
            return Math.min(
              APP_CONFIG.reconnectBaseDelay * Math.pow(2, retryContext.previousRetryCount),
              30000
            );
          },
        })
        .configureLogging(__DEV__ ? LogLevel.Information : LogLevel.Warning)
        .build();

      connection.onclose(() => {
        setIsConnected(false);
        setError('Connection closed');
      });

      connection.onreconnecting(() => {
        setIsConnected(false);
      });

      connection.onreconnected(() => {
        setIsConnected(true);
        setError(null);
        retryCountRef.current = 0;
      });

      await connection.start();
      connectionRef.current = connection;
      setIsConnected(true);
      setError(null);
      retryCountRef.current = 0;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Connection failed');
      setIsConnected(false);
    }
  }, [hubPath]);

  const disconnect = useCallback(async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop();
      connectionRef.current = null;
      setIsConnected(false);
    }
  }, []);

  const invoke = useCallback(
    async <T = void>(methodName: string, ...args: unknown[]): Promise<T | undefined> => {
      if (connectionRef.current?.state !== HubConnectionState.Connected) {
        if (__DEV__) console.warn(`Cannot invoke ${methodName}: not connected`);
        return undefined;
      }
      return connectionRef.current.invoke<T>(methodName, ...args);
    },
    []
  );

  const on = useCallback(
    (methodName: string, callback: (...args: unknown[]) => void) => {
      connectionRef.current?.on(methodName, callback);
      return () => connectionRef.current?.off(methodName, callback);
    },
    []
  );

  useEffect(() => {
    if (enabled) {
      connect();
    }
    return () => {
      disconnect();
    };
  }, [enabled, connect, disconnect]);

  return { isConnected, error, invoke, on, connect, disconnect };
}
