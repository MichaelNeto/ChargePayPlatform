import { apiGet, apiPost } from "../../shared/api/client";

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
  return apiPost<LoginResult>("/api/auth/login", payload);
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
  return apiPost<CadastroResult>("/api/users", payload);
}

export interface WalletTransactionResult {
  transactionId: string;
  type: string;
  description: string;
  amount: number;
  createdAt: string;
}

export interface WalletSummaryResult {
  walletId: string;
  userId: string;
  balance: number;
  availableValues: number[];
  transactions: WalletTransactionResult[];
}

export interface RechargeResult {
  rechargeId: string;
  walletId: string;
  amount: number;
  status: string;
  qrCode: string;
  createdAt: string;
  paidAt?: string | null;
}

export function getWallet() {
  return apiGet<WalletSummaryResult>("/api/wallet", { auth: true });
}

export function createRecharge(amount: number) {
  return apiPost<RechargeResult>(
    "/api/wallet/recharges",
    { amount },
    { auth: true },
  );
}

export function confirmRecharge(rechargeId: string) {
  return apiPost<RechargeResult>(
    `/api/wallet/recharges/${rechargeId}/confirm`,
    {},
    { auth: true },
  );
}
