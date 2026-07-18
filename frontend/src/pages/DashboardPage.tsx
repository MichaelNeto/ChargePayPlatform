import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../shared/store/authStore';
import { decodeJwt } from '../shared/lib/jwt';
import './dashboard.css';

const modules = [
  { title: 'Carteira', description: 'Saldo, recargas e histórico financeiro.', icon: '💳' },
  { title: 'Sessões de recarga', description: 'Acompanhe suas recargas em andamento e finalizadas.', icon: '⚡' },
  { title: 'Faturas', description: 'Cobranças geradas e comprovantes.', icon: '🧾' },
  { title: 'Notificações', description: 'Avisos importantes sobre sua conta.', icon: '🔔' },
];

export default function DashboardPage() {
  const navigate = useNavigate();
  const token = useAuthStore((s) => s.token);
  const storeEmail = useAuthStore((s) => s.user?.email);
  const logout = useAuthStore((s) => s.logout);

  const claims = useMemo(() => (token ? decodeJwt(token) : null), [token]);
  const displayName = claims?.unique_name || storeEmail || 'Cliente ChargePay';
  const email = claims?.email || storeEmail || '';

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="dash-page">
      <header className="dash-header">
        <div className="dash-brand">
          <span className="dash-brand-mark">⚡</span>
          <span className="dash-brand-name">
            ChargePay
          </span>
        </div>
        <button className="dash-logout-button" onClick={handleLogout}>
          Sair
        </button>
      </header>

      <main className="dash-content">
       <section className="dash-welcome">
          <div className="dash-welcome-text">
            <h1>Olá, {displayName} 👋</h1>
            {email && <p className="dash-welcome-email">{email}</p>}
            <p className="dash-welcome-subtitle">
              Sua conta está ativa. Em breve, sua carteira e recargas aparecerão aqui.
            </p>
          </div>
          <svg className="dash-welcome-illustration" viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg">
            <ellipse cx="100" cy="165" rx="70" ry="8" fill="rgba(0,0,0,0.25)" />
            <rect x="35" y="95" width="130" height="45" rx="12" fill="#22c55e" />
            <path d="M45 95 L60 65 Q65 58 75 58 L125 58 Q135 58 140 65 L155 95 Z" fill="#16a34a" />
            <rect x="70" y="68" width="25" height="22" rx="3" fill="#bbf7d0" opacity="0.9" />
            <rect x="105" y="68" width="25" height="22" rx="3" fill="#bbf7d0" opacity="0.9" />
            <circle cx="65" cy="140" r="16" fill="#0f172a" />
            <circle cx="65" cy="140" r="7" fill="#64748b" />
            <circle cx="135" cy="140" r="16" fill="#0f172a" />
            <circle cx="135" cy="140" r="7" fill="#64748b" />
            <rect x="90" y="105" width="20" height="26" rx="4" fill="#fde047" />
            <path d="M100 108 L94 120 L100 120 L98 128 L107 114 L101 114 Z" fill="#facc15" />
            <circle cx="150" cy="80" r="3" fill="#fde047" opacity="0.8" />
            <circle cx="160" cy="95" r="2" fill="#fde047" opacity="0.6" />
            <circle cx="145" cy="105" r="2.5" fill="#fde047" opacity="0.7" />
          </svg>
        </section>

        <section className="dash-grid">
          {modules.map((mod) => (
            <div key={mod.title} className="dash-card dash-card-disabled">
              <span className="dash-card-icon">{mod.icon}</span>
              <h3>{mod.title}</h3>
              <p>{mod.description}</p>
              <span className="dash-card-badge">Em breve</span>
            </div>
          ))}
        </section>
      </main>
    </div>
  );
}