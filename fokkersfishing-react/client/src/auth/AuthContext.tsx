import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, getToken, setToken } from '../api/client';
import type { AuthResponse, UserInfo } from '../api/types';

interface AuthContextValue {
  user: UserInfo | null;
  loading: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isUser: boolean;
  hasRole: (...roles: string[]) => boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, userName: string, password: string) => Promise<void>;
  loginExternal: (provider: string) => void;
  applyToken: (token: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);

  const loadUser = useCallback(async () => {
    if (!getToken()) {
      setUser(null);
      setLoading(false);
      return;
    }
    try {
      const { data } = await api.get<UserInfo>('/auth/user');
      setUser(data);
    } catch {
      setToken(null);
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadUser();
  }, [loadUser]);

  const login = useCallback(async (email: string, password: string) => {
    const { data } = await api.post<AuthResponse>('/auth/login', { email, password });
    setToken(data.token);
    setUser(data.user);
  }, []);

  const register = useCallback(async (email: string, userName: string, password: string) => {
    const { data } = await api.post<AuthResponse>('/auth/register', { email, userName, password });
    setToken(data.token);
    setUser(data.user);
  }, []);

  const applyToken = useCallback(
    async (token: string) => {
      setToken(token);
      await loadUser();
    },
    [loadUser],
  );

  const loginExternal = useCallback((provider: string) => {
    const returnUrl = '/';
    window.location.assign(`/auth/external/${provider}?returnUrl=${encodeURIComponent(returnUrl)}`);
  }, []);

  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
    window.location.assign('/');
  }, []);

  const value = useMemo<AuthContextValue>(() => {
    const roles = user?.roles ?? [];
    const hasRole = (...wanted: string[]) => wanted.some((r) => roles.includes(r));
    return {
      user,
      loading,
      isAuthenticated: !!user?.isAuthenticated,
      isAdmin: roles.includes('Administrator'),
      isUser: hasRole('Administrator', 'User'),
      hasRole,
      login,
      register,
      loginExternal,
      applyToken,
      logout,
    };
  }, [user, loading, login, register, loginExternal, applyToken, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
