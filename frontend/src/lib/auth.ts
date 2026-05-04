import { API_BASE_URL, buildApiUrl } from "./config";

export { API_BASE_URL } from "./config";

export const LOGIN_ENDPOINT = buildApiUrl("auth/login");
export const AUTH_STORAGE_KEY = "restaurant.auth.session";

export type LoginRequest = {
  userNameOrEmail: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  userName: string;
  email: string;
  fullName: string;
  roles: string[];
};

export function loadAuthSession(): LoginResponse | null {
  if (typeof window === "undefined") {
    return null;
  }

  const rawValue = window.localStorage.getItem(AUTH_STORAGE_KEY);
  if (!rawValue) {
    return null;
  }

  try {
    return JSON.parse(rawValue) as LoginResponse;
  } catch {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function saveAuthSession(session: LoginResponse): void {
  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}

export function clearAuthSession(): void {
  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}
