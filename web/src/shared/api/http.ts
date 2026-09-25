import axios from 'axios';

/**
 * The one Axios instance for the ASP.NET Core API. Auth is plugged in by the app at start-up
 * (configureHttpAuth) so this shared module never depends on the auth store.
 */
export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
});

interface HttpAuthHooks {
  getToken: () => string | null;
  onUnauthorized: () => void;
}

let authHooks: HttpAuthHooks = { getToken: () => null, onUnauthorized: () => undefined };

export function configureHttpAuth(hooks: HttpAuthHooks): void {
  authHooks = hooks;
}

http.interceptors.request.use((config) => {
  const token = authHooks.getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

http.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    // A 401 on login means wrong credentials, not an expired session.
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      !error.config?.url?.endsWith('/api/auth/login')
    ) {
      authHooks.onUnauthorized();
    }
    return Promise.reject(error);
  },
);
