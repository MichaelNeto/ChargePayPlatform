import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  confirmRecharge,
  createRecharge,
  type WalletSummaryResult,
} from "../features/auth/api";
import { useAuthStore } from "../shared/store/authStore";
import { useWalletStore } from "../shared/store/walletStore";
import { decodeJwt } from "../shared/lib/jwt";
import "./dashboard.css";

const predefinedAmounts = [50, 100, 200, 300];

const quickAccessCards = [
  {
    icon: "💳",
    title: "Carteira",
    description: "Saldo, recargas e histórico financeiro.",
    path: "/dashboard/wallet",
  },
  {
    icon: "⚡",
    title: "Sessões de recarga",
    description: "Acompanhe suas recargas em andamento e finalizadas.",
    path: "/dashboard/sessions",
  },
  {
    icon: "🧾",
    title: "Faturas",
    description: "Cobranças geradas e comprovantes.",
    path: "/dashboard/invoices",
  },
  {
    icon: "🔔",
    title: "Notificações",
    description: "Avisos importantes sobre sua conta.",
    path: "/dashboard/notifications",
  },
];

function WalletSummaryHeader({
  summary,
  loading,
}: {
  summary: WalletSummaryResult | null;
  loading: boolean;
}) {
  const transactions = summary?.transactions ?? [];
  const latestTransaction = [...transactions].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  )[0];

  return (
    <div className="dash-wallet-mini-summary">
      <div className="dash-wallet-mini-header">
        <span className="dash-wallet-mini-label">Carteira Digital</span>
        <span className="dash-wallet-mini-status">Ativa</span>
      </div>

      <div className="dash-wallet-mini-body">
        <div className="dash-wallet-mini-balance-group">
          <span className="dash-wallet-mini-caption">Saldo disponível</span>
          <strong className="dash-wallet-mini-balance">
            {loading
              ? "Carregando..."
              : `R$ ${(summary?.balance ?? 0).toFixed(2)}`}
          </strong>
        </div>

        <div className="dash-wallet-mini-stats">
          <span>
            {transactions.length} transação
            {transactions.length === 1 ? "" : "ões"}
          </span>
          <span>
            {latestTransaction
              ? new Date(latestTransaction.createdAt).toLocaleDateString(
                  "pt-BR",
                )
              : "Sem movimentação"}
          </span>
        </div>
      </div>
    </div>
  );
}

function DashboardShell({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}) {
  const navigate = useNavigate();
  const logout = useAuthStore((s) => s.logout);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="dash-page">
      <header className="dash-header">
        <div className="dash-brand">
          <span className="dash-brand-mark">⚡</span>
          <span className="dash-brand-name">ChargePay</span>
        </div>
        <button className="dash-logout-button" onClick={handleLogout}>
          Sair
        </button>
      </header>

      <main className="dash-content">
        <section className="dash-detail-header">
          <div className="dash-detail-header-text">
            <button
              className="dash-back-button"
              onClick={() => navigate("/dashboard")}
            >
              ← Voltar
            </button>
            <h1>{title}</h1>
            {subtitle && <p>{subtitle}</p>}
          </div>
        </section>

        {children}
      </main>
    </div>
  );
}

export default function DashboardPage() {
  const navigate = useNavigate();
  const logout = useAuthStore((s) => s.logout);
  const token = useAuthStore((s) => s.token);
  const storeEmail = useAuthStore((s) => s.user?.email);
  const walletSummary = useWalletStore((s) => s.summary);
  const walletLoading = useWalletStore((s) => s.loading);
  const loadWalletSummary = useWalletStore((s) => s.load);

  const claims = useMemo(() => (token ? decodeJwt(token) : null), [token]);
  const displayName = claims?.unique_name || storeEmail || "Cliente ChargePay";
  const email = claims?.email || storeEmail || "";

  useEffect(() => {
    loadWalletSummary();
  }, [loadWalletSummary]);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="dash-page">
      <header className="dash-header">
        <div className="dash-brand">
          <span className="dash-brand-mark">⚡</span>
          <span className="dash-brand-name">ChargePay</span>
        </div>
        <button className="dash-logout-button" onClick={handleLogout}>
          Sair
        </button>
      </header>

      <main className="dash-content">
        <section className="dash-welcome">
          <div className="dash-welcome-text">
            <h1>Olá, {displayName.toUpperCase()} 👋</h1>
            {email && <p className="dash-welcome-email">{email}</p>}
            <p className="dash-welcome-subtitle">
              Sua conta está ativa. Em breve, sua carteira e recargas aparecerão
              aqui.
            </p>
          </div>

          <div className="dash-welcome-side">
            <div className="dash-car-illustration" aria-hidden="true">
              <div className="dash-car-body">
                <div className="dash-car-window" />
                <div className="dash-car-light" />
                <div className="dash-car-wheel wheel-front" />
                <div className="dash-car-wheel wheel-back" />
              </div>
            </div>

            <WalletSummaryHeader
              summary={walletSummary}
              loading={walletLoading}
            />
          </div>
        </section>

        <section className="dash-feature-grid">
          {quickAccessCards.map((card) => (
            <button
              key={card.title}
              type="button"
              className="dash-feature-card dash-feature-card-button"
              onClick={() => navigate(card.path)}
            >
              <div className="dash-feature-top">
                <div className="dash-feature-icon">{card.icon}</div>
                <span className="dash-feature-badge">Em breve</span>
              </div>

              <div className="dash-feature-content">
                <h3>{card.title}</h3>
                <p>{card.description}</p>
              </div>
            </button>
          ))}
        </section>
      </main>
    </div>
  );
}

export function WalletDashboardPage() {
  const token = useAuthStore((s) => s.token);
  const storeEmail = useAuthStore((s) => s.user?.email);
  const walletSummary = useWalletStore((s) => s.summary);
  const walletLoading = useWalletStore((s) => s.loading);
  const loadWallet = useWalletStore((s) => s.load);

  const claims = useMemo(() => (token ? decodeJwt(token) : null), [token]);
  const displayName = claims?.unique_name || storeEmail || "Cliente ChargePay";

  const [pendingRecharge, setPendingRecharge] = useState<any>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const balance = walletSummary?.balance ?? 0;
  const transactions = walletSummary?.transactions ?? [];
  const loading = walletLoading;

  useEffect(() => {
    loadWallet();
  }, [loadWallet]);

  const handleCreateRecharge = async (amount: number) => {
    setSubmitting(true);
    setError(null);

    try {
      const result = await createRecharge(amount);
      if (!result.success || !result.data) {
        setError(result.message || "Não foi possível gerar a cobrança.");
        return;
      }

      setPendingRecharge(result.data);
      await loadWallet();
    } catch {
      setError("Não foi possível gerar a cobrança Pix simulada.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleConfirmPayment = async () => {
    if (!pendingRecharge) return;

    setSubmitting(true);
    setError(null);

    try {
      const result = await confirmRecharge(pendingRecharge.rechargeId);
      if (!result.success || !result.data) {
        setError(result.message || "Não foi possível confirmar o pagamento.");
        return;
      }

      setPendingRecharge(null);
      await loadWallet();
    } catch {
      setError("Não foi possível confirmar o pagamento simulado.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <DashboardShell
      title="Carteira digital"
      subtitle="Saldo, recarga e histórico financeiro."
    >
      <section className="dash-wallet-detail">
        <div className="dash-wallet-summary">
          <div>
            <span className="dash-wallet-label">Olá, {displayName}</span>
            <h2>Saldo disponível</h2>
          </div>
          <strong className="dash-wallet-balance-detail">
            {loading ? "Carregando..." : `R$ ${balance.toFixed(2)}`}
          </strong>
        </div>

        <div className="dash-wallet-merged-body">
          <div className="dash-wallet-section">
            <h3>Recarga via Pix</h3>
            <p>Escolha um valor pré-definido e gere o QR Code simulado.</p>

            <div className="dash-amount-grid">
              {predefinedAmounts.map((amount) => (
                <button
                  key={amount}
                  type="button"
                  className="dash-amount-button"
                  onClick={() => handleCreateRecharge(amount)}
                  disabled={submitting}
                >
                  R$ {amount}
                </button>
              ))}
            </div>

            {error && <div className="dash-banner-error">{error}</div>}

            {pendingRecharge && (
              <div className="dash-qr-box">
                <div className="dash-qr-code" aria-label="QR Code simulado Pix">
                  {pendingRecharge.qrCode}
                </div>
                <div className="dash-qr-meta">
                  <strong>
                    Valor: R$ {Number(pendingRecharge.amount).toFixed(2)}
                  </strong>
                  <span>Status: {pendingRecharge.status}</span>
                </div>
                <button
                  type="button"
                  className="dash-confirm-button"
                  onClick={handleConfirmPayment}
                  disabled={submitting}
                >
                  {submitting
                    ? "Confirmando..."
                    : "Simular pagamento confirmado"}
                </button>
              </div>
            )}
          </div>

          <div className="dash-wallet-section">
            <h3>Histórico financeiro</h3>
            <ul className="dash-transaction-list">
              {transactions.length === 0 ? (
                <li className="dash-empty-state">
                  Nenhuma transação registrada.
                </li>
              ) : (
                transactions.map((tx) => (
                  <li key={tx.transactionId} className="dash-transaction-item">
                    <div>
                      <strong>{tx.description}</strong>
                      <small>
                        {new Date(tx.createdAt).toLocaleString("pt-BR")}
                      </small>
                    </div>
                    <span
                      className={
                        tx.type === "Credit" ? "dash-credit" : "dash-debit"
                      }
                    >
                      {tx.type === "Credit" ? "+" : "-"}R${" "}
                      {Number(tx.amount).toFixed(2)}
                    </span>
                  </li>
                ))
              )}
            </ul>
          </div>
        </div>
      </section>
    </DashboardShell>
  );
}

export function SessionsDashboardPage() {
  return (
    <DashboardShell
      title="Sessões de recarga"
      subtitle="Acompanhe suas recargas em andamento e finalizadas."
    >
      <section className="dash-placeholder-panel">
        <p>Em breve: painel de sessões de recarga.</p>
      </section>
    </DashboardShell>
  );
}

export function InvoiceDashboardPage() {
  return (
    <DashboardShell
      title="Faturas"
      subtitle="Cobranças geradas e comprovantes."
    >
      <section className="dash-placeholder-panel">
        <p>Em breve: área de faturas.</p>
      </section>
    </DashboardShell>
  );
}

export function NotificationDashboardPage() {
  return (
    <DashboardShell
      title="Notificações"
      subtitle="Avisos importantes sobre sua conta."
    >
      <section className="dash-placeholder-panel">
        <p>Em breve: central de notificações.</p>
      </section>
    </DashboardShell>
  );
}
