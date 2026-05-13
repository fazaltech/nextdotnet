const defaultApiBaseUrl = "http://localhost:5179/api";

function normalizeUrl(url: string): string {
  return url.trim().replace(/\/+$/, "");
}

export const API_BASE_URL = normalizeUrl(
  process.env.NEXT_PUBLIC_API_BASE_URL || defaultApiBaseUrl,
);

export function buildApiUrl(path: string): string {
  return `${API_BASE_URL}/${path.replace(/^\/+/, "")}`;
}
