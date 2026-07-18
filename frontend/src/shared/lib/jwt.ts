interface JwtPayload {
  sub?: string;
  email?: string;
  unique_name?: string;
  exp?: number;
  [key: string]: unknown;
}

export function decodeJwt(token: string): JwtPayload | null {
  try {
    const payload = token.split('.')[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded) as JwtPayload;
  } catch {
    return null;
  }
}