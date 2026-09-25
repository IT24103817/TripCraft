import type { ReactNode } from 'react';
import type { Role } from '@/shared/api/types';
import { useAuthStore } from './authStore';
import { ForbiddenPage } from './ForbiddenPage';

/** Renders the 403 page for a signed-in user whose role may not see this screen. The API checks again. */
export function RoleGuard({ roles, children }: { roles: Role[]; children: ReactNode }) {
  const role = useAuthStore((s) => s.user?.role);
  if (!role || !roles.includes(role)) return <ForbiddenPage />;
  return <>{children}</>;
}
