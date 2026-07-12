import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../shared/store/authStore';

export default function DashboardPage() {
  const navigate = useNavigate();
  const logout = useAuthStore((s) => s.logout);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div style={{ padding: 40 }}>
      <h1>Bem-vindo à ChargePay 👋</h1>
      <button onClick={handleLogout}>Sair</button>
    </div>
  );
}