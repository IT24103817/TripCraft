/** Shape of every list response from the API (PagedResult<T> in C#). */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
}

/** RFC 7807 body returned by the API's exception middleware. */
export interface ProblemDetails {
  title?: string;
  detail?: string | null;
  status?: number;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

/** UserRole in C#; the JWT "role" claim. */
export type Role = 'Tourist' | 'Guide' | 'OperationsManager' | 'Admin';
export const ROLES: Role[] = ['Tourist', 'Guide', 'OperationsManager', 'Admin'];
export const STAFF_ROLES: Role[] = ['OperationsManager', 'Admin'];
