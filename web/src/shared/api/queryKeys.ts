/**
 * Root query keys. Features build their keys under these roots, so a mutation in one feature can
 * invalidate another feature's cache (e.g. approving a quotation refreshes trips) without importing it.
 */
export const queryRoots = {
  trips: 'trips',
  attractions: 'attractions',
  workflows: 'workflows',
  reports: 'reports',
  users: 'users',
  auditLogs: 'auditLogs',
} as const;
