import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  exp: number;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  isAdmin: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

const AUTH_API_URL = import.meta.env.VITE_AUTH_API_URL || 'http://localhost:5004/api/v1.0/auth';

function parseToken(token: string): User {
  const decoded = jwtDecode<JwtPayload>(token);
  const roleClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const role = Array.isArray(roleClaim) ? roleClaim[0] : roleClaim ?? 'User';
  return {
    id: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
    email: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
    firstName: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'],
    lastName: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'],
    role,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'));
  const [refreshToken, setRefreshToken] = useState<string | null>(() => localStorage.getItem('refreshToken'));
  const [user, setUser] = useState<User | null>(() => {
    const t = localStorage.getItem('token');
    if (!t) return null;
    try {
      return parseToken(t);
    } catch {
      return null;
    }
  });

  const logout = useCallback(() => {
    setToken(null);
    setRefreshToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
  }, []);

  // Auto-refresh token before expiry
  useEffect(() => {
    if (!token || !refreshToken) return;
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const expiresIn = decoded.exp * 1000 - Date.now() - 60_000; // refresh 1 min before expiry
      if (expiresIn <= 0) {
        // Token already expired or about to, refresh now
        refreshTokens();
        return;
      }
      const timer = setTimeout(refreshTokens, expiresIn);
      return () => clearTimeout(timer);
    } catch {
      logout();
    }

    async function refreshTokens() {
      try {
        const res = await fetch(`${AUTH_API_URL}/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken }),
        });
        if (!res.ok) { logout(); return; }
        const data = await res.json();
        setToken(data.token);
        setRefreshToken(data.refreshToken);
        setUser(parseToken(data.token));
        localStorage.setItem('token', data.token);
        localStorage.setItem('refreshToken', data.refreshToken);
      } catch {
        logout();
      }
    }
  }, [token, refreshToken, logout]);

  const login = useCallback(async (email: string, password: string) => {
    const res = await fetch(`${AUTH_API_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    if (!res.ok) {
      throw new Error(res.status === 401 ? 'Invalid email or password' : 'Login failed');
    }
    const data = await res.json();
    const parsed = parseToken(data.token);
    if (parsed.role !== 'Admin') {
      throw new Error('Access denied. Admin role required.');
    }
    setToken(data.token);
    setRefreshToken(data.refreshToken);
    setUser(parsed);
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
  }, []);

  const isAdmin = user?.role === 'Admin';

  return (
    <AuthContext.Provider value={{ user, token, isAdmin, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
