import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import type { AuthResponse, UserDto } from './types';

interface AuthState {
  token: string | null;
  expiresAt: string | null;
  user: UserDto | null;
  login: (response: AuthResponse) => void;
  logout: () => void;
}

/** Session only: kept in sessionStorage, so closing the tab signs the user out. */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      expiresAt: null,
      user: null,
      login: (response) =>
        set({ token: response.accessToken, expiresAt: response.expiresAt, user: response.user }),
      logout: () => set({ token: null, expiresAt: null, user: null }),
    }),
    { name: 'tripcraft-auth', storage: createJSONStorage(() => sessionStorage) },
  ),
);

export function isSessionValid(state: Pick<AuthState, 'token' | 'expiresAt'>, now = Date.now()): boolean {
  return Boolean(state.token) && (!state.expiresAt || new Date(state.expiresAt).getTime() > now);
}
