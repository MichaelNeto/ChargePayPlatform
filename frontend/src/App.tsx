import { Navigate, Route, Routes } from 'react-router-dom';
import LoginPage from './features/auth/LoginPage';
import CadastroPage from './features/auth/CadastroPage';
import DashboardPage from './pages/DashboardPage';
import { useAuthStore } from './shared/store/authStore';

function RequireAuth({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/cadastro" element={<CadastroPage />} />
      <Route
        path="/dashboard"
        element={
          <RequireAuth>
            <DashboardPage />
          </RequireAuth>
        }
      />
    </Routes>
  );
}


// import { Routes, Route, Navigate } from 'react-router-dom';
// import { AuthPage } from './features/auth/AuthPage';
// import { OnboardingPage } from './features/onboarding/OnboardingPage';

// function App() {
//   return (
//     <Routes>
//       <Route path="/" element={<Navigate to="/auth" replace />} />
//       <Route path="/auth" element={<AuthPage />} />
//       <Route path="/onboarding" element={<OnboardingPage />} />
//     </Routes>
//   );
// }

// export default App;
