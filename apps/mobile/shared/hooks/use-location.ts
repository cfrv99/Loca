import { useEffect, useState, useCallback } from 'react';
import * as Location from 'expo-location';

interface LocationState {
  latitude: number;
  longitude: number;
  isLoading: boolean;
  error: string | null;
  permissionStatus: Location.PermissionStatus | null;
}

const BAKU_DEFAULT = { latitude: 40.4093, longitude: 49.8671 };

export function useLocation() {
  const [state, setState] = useState<LocationState>({
    latitude: BAKU_DEFAULT.latitude,
    longitude: BAKU_DEFAULT.longitude,
    isLoading: true,
    error: null,
    permissionStatus: null,
  });

  const requestPermission = useCallback(async () => {
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      setState((prev) => ({ ...prev, permissionStatus: status }));

      if (status !== Location.PermissionStatus.GRANTED) {
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: 'Konum icazesi verilmedi',
        }));
        return false;
      }
      return true;
    } catch {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: 'Konum icazesi sorgulanarkən xəta',
      }));
      return false;
    }
  }, []);

  const fetchLocation = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    const granted = await requestPermission();
    if (!granted) return;

    try {
      const location = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
      });

      setState({
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        isLoading: false,
        error: null,
        permissionStatus: Location.PermissionStatus.GRANTED,
      });
    } catch {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: 'Konum alınamadı',
      }));
    }
  }, [requestPermission]);

  useEffect(() => {
    fetchLocation();
  }, [fetchLocation]);

  return {
    latitude: state.latitude,
    longitude: state.longitude,
    isLoading: state.isLoading,
    error: state.error,
    permissionStatus: state.permissionStatus,
    refetch: fetchLocation,
  };
}
