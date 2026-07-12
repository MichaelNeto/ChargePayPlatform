import { apiPost } from '../../shared/api/client';

export interface LoginPayload {
  email: string;
  password: string;
}

export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export function login(payload: LoginPayload) {
  return apiPost<LoginResult>('/api/auth/login', payload);
}

export interface CadastroPayload {
  firstName: string;
  lastName: string;
  document: string;
  phone: string;
  email: string;
  birthDate: string;
  password: string;
}

export interface CadastroResult {
  customerId: string;
  userId: string;
}

export function cadastrar(payload: CadastroPayload) {
  return apiPost<CadastroResult>('/api/users', payload);
}