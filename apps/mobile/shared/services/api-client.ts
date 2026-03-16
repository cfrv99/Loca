import axios from 'axios';
import { CONFIG } from '../constants/config';
import { useAuthStore } from '../stores/auth-store';
import type { ApiResponse } from '../types';

export const api = axios.create({
  baseURL: `${CONFIG.API_URL}/api/v1`,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor: attach JWT
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: handle 401 auto-refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = useAuthStore.getState().refreshToken;
      if (!refreshToken) {
        useAuthStore.getState().logout();
        return Promise.reject(error);
      }

      try {
        const res = await axios.post<ApiResponse<{ accessToken: string; refreshToken: string }>>(
          `${CONFIG.API_URL}/api/v1/auth/refresh`,
          { refreshToken }
        );

        if (res.data.success && res.data.data) {
          useAuthStore.getState().setTokens(res.data.data.accessToken, res.data.data.refreshToken);
          originalRequest.headers.Authorization = `Bearer ${res.data.data.accessToken}`;
          return api(originalRequest);
        }
      } catch {
        useAuthStore.getState().logout();
      }
    }

    return Promise.reject(error);
  }
);
