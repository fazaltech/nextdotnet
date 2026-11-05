import { clearAuthSession, createAuthHeaders, loadAuthSession } from "./auth";
import { buildApiUrl } from "./config";

export async function fetchWithAuth<T>(url: string): Promise<T> {
  const session = loadAuthSession();
  if (!session) throw new Error("AUTH_REQUIRED");

  const response = await fetch(url, {
    headers: createAuthHeaders(session.accessToken),
  });

  if (response.status === 401) {
    clearAuthSession();
    throw new Error("AUTH_EXPIRED");
  }

  if (!response.ok) throw new Error(`REQUEST_FAILED:${response.status}`);
  return (await response.json()) as T;
}

export async function apiGet<T>(path: string): Promise<T> {
  return fetchWithAuth<T>(buildApiUrl(path));
}
