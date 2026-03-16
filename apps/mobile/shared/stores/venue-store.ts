import { create } from 'zustand';

interface VenueState {
  currentVenueId: string | null;
  currentVenueName: string | null;
  checkInId: string | null;
  isCheckedIn: boolean;
  isAnonymous: boolean;

  setCheckIn: (venueId: string, venueName: string, checkInId: string, isAnonymous: boolean) => void;
  clearCheckIn: () => void;
}

export const useVenueStore = create<VenueState>((set) => ({
  currentVenueId: null,
  currentVenueName: null,
  checkInId: null,
  isCheckedIn: false,
  isAnonymous: false,

  setCheckIn: (venueId, venueName, checkInId, isAnonymous) =>
    set({
      currentVenueId: venueId,
      currentVenueName: venueName,
      checkInId,
      isCheckedIn: true,
      isAnonymous,
    }),

  clearCheckIn: () =>
    set({
      currentVenueId: null,
      currentVenueName: null,
      checkInId: null,
      isCheckedIn: false,
      isAnonymous: false,
    }),
}));
