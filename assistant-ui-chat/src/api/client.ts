import { API_BASE } from "../config";

async function parseJsonSafe(res: Response) {
  const text = await res.text();
  try {
    return text ? JSON.parse(text) : null;
  } catch {
    return text;
  }
}

async function request(path: string, options: RequestInit = {}) {
  const url = path.startsWith("http") ? path : `${API_BASE}${path}`;
  const res = await fetch(url, { credentials: "include", ...options });

  if (!res.ok) {
    const body = await parseJsonSafe(res).catch(() => null);
    const message =
      (body && (body.error || body.message)) ||
      `HTTP error ${res.status} ${res.statusText}`;
    const err = new Error(message);
    // attach response details for richer debugging
    // @ts-ignore
    err.status = res.status;
    // @ts-ignore
    err.body = body;
    throw err;
  }

  // 204/empty responses
  if (res.status === 204) return null;
  return await parseJsonSafe(res);
}

export const apiClient = {
  get: (path: string) => request(path, { method: "GET" }),
  post: (path: string, body?: any) =>
    request(path, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: body ? JSON.stringify(body) : undefined,
    }),
  put: (path: string, body?: any) =>
    request(path, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: body ? JSON.stringify(body) : undefined,
    }),
  del: (path: string) =>
    request(path, {
      method: "DELETE",
    }),
  rawRequest: request,
};
