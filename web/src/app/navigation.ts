import type { Role } from '@/shared/api/types';
import type { NavGroup } from '@/shared/components/Sidebar';

const MANAGER: Role[] = ['OperationsManager'];
const STAFF: Role[] = ['OperationsManager', 'Admin'];
const ADMIN: Role[] = ['Admin'];

/**
 * The four sidebar groups and who may open each link (matched to the API's [Authorize] rules):
 * operations, resources and reports are Operations Manager work; the dashboard and the agent runs are readable
 * by Admins too; users, the audit log and the settings are Admin only (separation of duties).
 * Attractions are opened from the Trips page.
 */
export const NAV_GROUPS: NavGroup[] = [
  {
    label: 'Operations',
    items: [
      { to: '/dashboard', label: 'Dashboard', roles: STAFF },
      { to: '/trips', label: 'Trips', roles: MANAGER },
      { to: '/approvals', label: 'Review queue', roles: MANAGER },
    ],
  },
  {
    label: 'Resources',
    items: [
      { to: '/resources/guides', label: 'Guides', roles: MANAGER },
      { to: '/resources/vehicles', label: 'Vehicles', roles: MANAGER },
      { to: '/resources/hotels', label: 'Hotels', roles: MANAGER },
      { to: '/availability', label: 'Availability', roles: MANAGER },
    ],
  },
  {
    label: 'Insights',
    items: [
      { to: '/reports', label: 'Reports', roles: MANAGER },
      { to: '/agent-runs', label: 'Agent runs', roles: STAFF },
    ],
  },
  {
    label: 'Admin',
    items: [
      { to: '/admin/users', label: 'Users', roles: ADMIN },
      { to: '/admin/audit-logs', label: 'Audit log', roles: ADMIN },
      { to: '/admin/settings', label: 'Settings', roles: ADMIN },
    ],
  },
];
