import { useAuthStore } from "../store/authStore";
import type { ApiResponse } from "../types/api";

const API_URL = import.meta.env.VITE_API_URL as string;

export async function apiRequest<T>(
  path: string,
  method: "GET" | "POST",
  body?: unknown,
  options: { auth?: boolean } = {},
): Promise<ApiResponse<T>> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (options.auth) {
    const token = useAuthStore.getState().token;
    if (token) headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  const json = (await response.json()) as ApiResponse<T>;
  return json;
}

export async function apiPost<T>(
  path: string,
  body: unknown,
  options: { auth?: boolean } = {},
): Promise<ApiResponse<T>> {
  return apiRequest<T>(path, "POST", body, options);
}

export async function apiGet<T>(
  path: string,
  options: { auth?: boolean } = {},
): Promise<ApiResponse<T>> {
  return apiRequest<T>(path, "GET", undefined, options);
}

// import axios from 'axios';

// export const apiClient = axios.create({
//   baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5247',
//   timeout: 10000,
//   headers: {
//     'Content-Type': 'application/json'
//   }
// });

// apiClient.interceptors.request.use((config) => {
//   const token = localStorage.getItem('chargepay_token');
//   if (token) {
//     config.headers.Authorization = `Bearer ${token}`;
//   }
//   return config;
// });
