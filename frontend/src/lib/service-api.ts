import { createAuthHeaders, loadAuthSession, clearAuthSession } from "./auth";
import { buildApiUrl } from "./config";

export async function serviceApi<T>(path: string, method = "GET", body?: unknown): Promise<T> {
  const session = loadAuthSession();
  if (!session) throw new Error("Please sign in.");
  const response = await fetch(buildApiUrl(path), {
    method, headers: createAuthHeaders(session.accessToken),
    body: body === undefined ? undefined : JSON.stringify(body), cache: "no-store",
  });
  if (response.status === 401) { clearAuthSession(); window.location.assign("/"); throw new Error("Your session has expired."); }
  if (!response.ok) {
    const error = await response.json().catch(() => null);
    throw new Error(error?.message || (response.status === 403 ? "Your role does not have permission for this action." : error?.title || `Request failed (${response.status}).`));
  }
  return response.status === 204 ? undefined as T : await response.json() as T;
}
