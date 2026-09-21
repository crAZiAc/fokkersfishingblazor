import axios from 'axios';

const TOKEN_KEY = 'fokkers.token';

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export function setToken(token: string | null) {
  try {
    if (token) localStorage.setItem(TOKEN_KEY, token);
    else localStorage.removeItem(TOKEN_KEY);
  } catch {
    /* ignore storage failures (private mode etc.) */
  }
}

// Single axios instance. Base URL is empty because the API is served from the
// same origin (single Azure Web App); in dev, Vite proxies the API prefixes.
export const api = axios.create({ baseURL: '' });

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// A 401 means the token is missing/expired: drop it and bounce to login.
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error?.response?.status === 401) {
      setToken(null);
      if (!window.location.pathname.startsWith('/login')) {
        window.location.assign('/login');
      }
    }
    return Promise.reject(error);
  },
);
