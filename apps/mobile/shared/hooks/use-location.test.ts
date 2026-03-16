import { renderHook } from '@testing-library/react-native';
import { useLocation } from './use-location';

describe('useLocation', () => {
  it('should initialize with Baku defaults', () => {
    const { result } = renderHook(() => useLocation());
    expect(result.current.latitude).toBe(40.4093);
    expect(result.current.longitude).toBe(49.8671);
  });

  it('should have loading state initially', () => {
    const { result } = renderHook(() => useLocation());
    expect(result.current.isLoading).toBe(true);
  });

  it('should have no error initially', () => {
    const { result } = renderHook(() => useLocation());
    expect(result.current.error).toBeNull();
  });

  it('should return refetch function', () => {
    const { result } = renderHook(() => useLocation());
    expect(typeof result.current.refetch).toBe('function');
  });
});
