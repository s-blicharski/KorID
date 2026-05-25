export interface JwtClaims {
  unique_name?: string;
  email?: string;
  role?: string | string[];
  exp?: number;
  [k: string]: unknown;
}

function base64UrlDecode(input: string): string | null {
  input = input.replace(/-/g, '+').replace(/_/g, '/');
  const pad = input.length % 4;
  if (pad === 2) input += '==';
  else if (pad === 3) input += '=';
  else if (pad !== 0) return null;
  try {
    return decodeURIComponent(
      Array.prototype.map
        .call(atob(input), (c: string) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
  } catch {
    return null;
  }
}

export function decodeToken(token: string | null): JwtClaims | null {
  if (!token) return null;
  const parts = token.split('.');
  if (parts.length < 2) return null;
  const json = base64UrlDecode(parts[1]!);
  if (!json) return null;
  try {
    return JSON.parse(json) as JwtClaims;
  } catch {
    return null;
  }
}

export function getRoles(token: string | null): string[] {
  const claims = decodeToken(token);
  if (!claims?.role) return [];
  return Array.isArray(claims.role) ? claims.role : [claims.role];
}

export function isExpired(token: string | null): boolean {
  const claims = decodeToken(token);
  if (!claims?.exp) return true;
  return Date.now() >= claims.exp * 1000;
}

export function hasRole(token: string | null, role: string): boolean {
  return getRoles(token).includes(role);
}

export function getUsername(token: string | null): string | null {
  return decodeToken(token)?.unique_name ?? null;
}

