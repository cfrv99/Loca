import { renderHook, act } from '@testing-library/react-native';
import { useAuthStore } from './auth-store';

describe('useAuthStore', () => {
  beforeEach(() => {
    useAuthStore.setState({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      isLoading: true,
    });
  });

  it('should initialize with default state', () => {
    const { result } = renderHook(() => useAuthStore());
    expect(result.current.accessToken).toBeNull();
    expect(result.current.refreshToken).toBeNull();
    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isLoading).toBe(true);
  });

  it('should set tokens and authenticate', () => {
    const { result } = renderHook(() => useAuthStore());
    act(() => {
      result.current.setTokens('access_token', 'refresh_token');
    });
    expect(result.current.accessToken).toBe('access_token');
    expect(result.current.refreshToken).toBe('refresh_token');
    expect(result.current.isAuthenticated).toBe(true);
  });

  it('should set user', () => {
    const { result } = renderHook(() => useAuthStore());
    const user = {
      id: 'test-id',
      email: 'test@example.com',
      displayName: 'Test User',
      dateOfBirth: '1998-01-01',
      gender: 'male',
      interests: [],
      purposes: [],
      vibePreferences: [],
      isOnboarded: true,
      isPremium: false,
      coinBalance: 0,
      createdAt: '2024-01-01T00:00:00Z',
    };
    act(() => {
      result.current.setUser(user);
    });
    expect(result.current.user).toEqual(user);
  });

  it('should logout and clear state', () => {
    const { result } = renderHook(() => useAuthStore());
    act(() => {
      result.current.setTokens('access', 'refresh');
      result.current.logout();
    });
    expect(result.current.accessToken).toBeNull();
    expect(result.current.refreshToken).toBeNull();
    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
  });

  it('should set loading state', () => {
    const { result } = renderHook(() => useAuthStore());
    act(() => {
      result.current.setLoading(false);
    });
    expect(result.current.isLoading).toBe(false);
  });
});
