import { STAFF_ROLES, type Role } from '@/shared/api/types';

export const isStaff = (role: Role | undefined): boolean => role !== undefined && STAFF_ROLES.includes(role);

/** Where to land after login: staff use this app, tourists and guides use the Flutter app. */
export const homeFor = (role: Role): string => (isStaff(role) ? '/dashboard' : '/mobile-app');
