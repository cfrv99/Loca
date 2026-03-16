import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../../../shared/services/api-client';
import { useVenueStore } from '../../../shared/stores/venue-store';
import type { ApiResponse, CheckInResultDto } from '../../../shared/types';

interface CheckInParams {
  qrPayload: string;
  lat: number;
  lng: number;
  deviceFingerprint: string;
  isAnonymous: boolean;
}

export function useCheckIn() {
  const queryClient = useQueryClient();
  const setCheckIn = useVenueStore((s) => s.setCheckIn);

  return useMutation({
    mutationFn: async (params: CheckInParams) => {
      const res = await api.post<ApiResponse<CheckInResultDto>>(
        '/checkin',
        params
      );
      if (!res.data.success || !res.data.data) {
        const errorCode = res.data.error?.code ?? 'UNKNOWN';
        const errorMsg = res.data.error?.message ?? 'Xəta baş verdi';
        throw new CheckInError(errorCode, errorMsg);
      }
      return res.data.data;
    },
    onSuccess: (data) => {
      setCheckIn(data.venueId, data.venueName, data.checkInId, data.isAnonymous);
      queryClient.invalidateQueries({ queryKey: ['venues'] });
    },
  });
}

export class CheckInError extends Error {
  code: string;

  constructor(code: string, message: string) {
    super(message);
    this.code = code;
    this.name = 'CheckInError';
  }
}
