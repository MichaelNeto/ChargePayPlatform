import { create } from 'zustand';

interface AuthUser {
  userId: string;
  email: string;
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (token: string, user: AuthUser) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  user: null,
  isAuthenticated: false,
  login: (token, user) => set({ token, user, isAuthenticated: true }),
  logout: () => set({ token: null, user: null, isAuthenticated: false }),
}));




// import { create } from 'zustand';

// type AuthState = {
//   isAuthenticated: boolean;
//   token: string | null;
//   user: { email: string } | null;
//   login: (token: string, email: string) => void;
//   logout: () => void;
// };

// export const useAuthStore = create<AuthState>((set) => ({
//   isAuthenticated: false,
//   token: null,
//   user: null,
//   login: (token, email) => {
//     localStorage.setItem('chargepay_token', token);
//     set({ isAuthenticated: true, token, user: { email } });
//   },
//   logout: () => {
//     localStorage.removeItem('chargepay_token');
//     set({ isAuthenticated: false, token: null, user: null });
//   }
// }));
