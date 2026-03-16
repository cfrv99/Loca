import { useEffect, useRef, useState } from 'react';
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
  HttpTransportType,
} from '@microsoft/signalr';
import { useAuthStore } from '../stores/auth-store';
import { CONFIG } from '../constants/config';

export function useSignalR(hubPath: string) {
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const { accessToken } = useAuthStore();
  const retryDelayRef = useRef(1000);

  useEffect(() => {
    if (!accessToken) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`${CONFIG.API_URL}${hubPath}`, {
        accessTokenFactory: () => accessToken,
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => {
          const delay = Math.min(retryDelayRef.current, 30000);
          retryDelayRef.current *= 2;
          return delay;
        },
      })
      .configureLogging(__DEV__ ? LogLevel.Information : LogLevel.Error)
      .build();

    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => {
      setIsConnected(true);
      retryDelayRef.current = 1000;
    });
    connection.onclose(() => setIsConnected(false));

    connection
      .start()
      .then(() => {
        setIsConnected(true);
        retryDelayRef.current = 1000;
      })
      .catch((err) => {
        if (__DEV__) console.error('SignalR connect failed:', err);
      });

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [accessToken, hubPath]);

  return { connection: connectionRef.current, isConnected };
}
