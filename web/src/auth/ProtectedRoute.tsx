import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { isSessionValid, useAuthStore } from './authStore';

/** Any signed-in user; otherwise go to /login and come back afterwards. */
export function ProtectedRoute() {
  const location = useLocation();
  const token = useAuthStore((s) => s.token);
  const expiresAt = useAuthStore((s) => s.expiresAt);

  if (!isSessionValid({ token, expiresAt })) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />;
  }
  return <Outlet />;
}
