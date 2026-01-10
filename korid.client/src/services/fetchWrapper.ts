export type FetchOptions = RequestInit & { auth?: boolean };

const prefix = '/api';

function joinPath(path: string) {
  if (!path.startsWith('/')) return `${prefix}/${path}`;
  return `${prefix}${path}`;
}

async function parseResponse(res: Response) {
  const text = await res.text();
  try {
    return text ? JSON.parse(text) : null;
  } catch {
    return text;
  }
}

async function handleResponse<T = unknown>(res: Response): Promise<T> {
  const data = await parseResponse(res) as T;
  if (!res.ok) {
    if (res.status === 401) {
      window.dispatchEvent(new CustomEvent('korid:unauthorized'));
    }
    throw { status: res.status, data };
  }
  return data;
}

export const fetchWrapper = {
  async get<T = unknown>(path: string, options: FetchOptions = {}): Promise<T> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json', ...(options.headers as Record<string, string> ?? {}) };
    if (options.auth !== false) {
      const token = localStorage.getItem('korid_token');
      if (token) headers.Authorization = `Bearer ${token}`;
    }
    const res = await fetch(joinPath(path), { ...options, method: 'GET', headers });
    return handleResponse<T>(res);
  },
  async post<T = unknown>(path: string, body?: unknown, options: FetchOptions = {}): Promise<T> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json', ...(options.headers as Record<string, string> ?? {}) };
    if (options.auth !== false) {
      const token = localStorage.getItem('korid_token');
      if (token) headers.Authorization = `Bearer ${token}`;
    }
    const res = await fetch(joinPath(path), { ...options, method: 'POST', body: body ? JSON.stringify(body) : undefined, headers });
    return handleResponse<T>(res);
  },
  async put<T = unknown>(path: string, body?: unknown, options: FetchOptions = {}): Promise<T> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json', ...(options.headers as Record<string, string> ?? {}) };
    if (options.auth !== false) {
      const token = localStorage.getItem('korid_token');
      if (token) headers.Authorization = `Bearer ${token}`;
    }
    const res = await fetch(joinPath(path), { ...options, method: 'PUT', body: body ? JSON.stringify(body) : undefined, headers });
    return handleResponse<T>(res);
  },
  async del<T = unknown>(path: string, options: FetchOptions = {}): Promise<T> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json', ...(options.headers as Record<string, string> ?? {}) };
    if (options.auth !== false) {
      const token = localStorage.getItem('korid_token');
      if (token) headers.Authorization = `Bearer ${token}`;
    }
    const res = await fetch(joinPath(path), { ...options, method: 'DELETE', headers });
    return handleResponse<T>(res);
  }
};
