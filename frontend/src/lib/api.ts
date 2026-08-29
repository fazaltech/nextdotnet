import { clearAuthSession, createAuthHeaders, loadAuthSession } from "./auth";
import { buildApiUrl } from "./config";

export const DEMO_ACCESS_TOKEN = "restaurant-demo-session";

export async function apiGet<T>(path: string, fallback: T): Promise<T> {
  const session = loadAuthSession();
  if (!session) throw new Error("AUTH_REQUIRED");

  if (session.accessToken === DEMO_ACCESS_TOKEN) {
    await new Promise((resolve) => window.setTimeout(resolve, 180));
    return fallback;
  }

  try {
    const response = await fetch(buildApiUrl(path), {
      headers: createAuthHeaders(session.accessToken),
    });

    if (response.status === 401) {
      clearAuthSession();
      throw new Error("AUTH_EXPIRED");
    }

    if (!response.ok) throw new Error(`REQUEST_FAILED:${response.status}`);
    return (await response.json()) as T;
  } catch (error) {
    if (
      error instanceof Error &&
      (error.message === "AUTH_REQUIRED" || error.message === "AUTH_EXPIRED")
    ) {
      throw error;
    }

    return fallback;
  }
}
