import type { NavItem } from '@/shared/components/Sidebar';

/**
 * Sidebar links and who may open them (PLAN.md section 2, matched to the API's [Authorize] rules):
 * trips, resources, approvals and reports are Operations Manager work; workflows are readable by
 * Admins too; user management and the audit log are Admin only (separation of duties).
 */
export const NAV_ITEMS: NavItem[] = [
  { to: '/', label: 'Dashboard', roles: ['OperationsManager', 'Admin'] },
  { to: '/approvals', label: 'Approvals', roles: ['OperationsManager'] },
  { to: '/trips', label: 'Trip requests', roles: ['OperationsManager'] },
  { to: '/attractions', label: 'Attractions', roles: ['OperationsManager'] },
  { to: '/resources/guides', label: 'Guides', roles: ['OperationsManager'] },
  { to: '/resources/vehicles', label: 'Vehicles', roles: ['OperationsManager'] },
  { to: '/resources/hotels', label: 'Hotels', roles: ['OperationsManager'] },
  { to: '/availability', label: 'Availability', roles: ['OperationsManager'] },
  { to: '/workflows', label: 'Agent workflows', roles: ['OperationsManager', 'Admin'] },
  { to: '/quotations', label: 'Quotations', roles: ['OperationsManager'] },
  { to: '/reports', label: 'Reports', roles: ['OperationsManager'] },
  { to: '/admin/users', label: 'Users', roles: ['Admin'] },
  { to: '/admin/audit-logs', label: 'Audit log', roles: ['Admin'] },
];
