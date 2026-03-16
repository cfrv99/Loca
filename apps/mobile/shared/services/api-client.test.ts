jest.mock('../stores/auth-store');
jest.mock('../constants/config', () => ({
  CONFIG: { API_URL: 'http://localhost:3000' },
}));

import { api } from './api-client';

describe('API Client', () => {
  it('should have request interceptor configured', () => {
    expect(api.interceptors.request).toBeDefined();
    expect(api.interceptors.request.use).toBeDefined();
  });

  it('should have response interceptor configured', () => {
    expect(api.interceptors.response).toBeDefined();
    expect(api.interceptors.response.use).toBeDefined();
  });

  it('should have correct baseURL and timeout', () => {
    expect(api.defaults.baseURL).toBe('http://localhost:3000/api/v1');
    expect(api.defaults.timeout).toBe(15000);
  });

  it('should have correct content type header', () => {
    expect((api.defaults.headers as any)['Content-Type']).toBe('application/json');
  });
});
