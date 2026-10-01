import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';

/** One row of GET /api/admin/audit-logs. before/after are JSON snapshots as stored. */
export interface AuditLogDto {
  id: string;
  at: string;
  actorEmail: string | null;
  actorRole: string | null;
  action: string;
  entity: string;
  entityId: string;
  before: string | null;
  after: string | null;
}

/** actor matches part of the user's email (case-insensitive); "system" means rows without a user. */
export interface AuditLogQuery {
  actor: string;
  entity: string;
  from: string;
  to: string;
  search: string;
  sort: string;
  page: number;
  pageSize: number;
}

export function useAuditLogs(query: AuditLogQuery) {
  const params = Object.fromEntries(Object.entries(query).filter(([, v]) => v !== ''));
  return useQuery({
    queryKey: [queryRoots.auditLogs, params],
    queryFn: async () => (await http.get<PagedResult<AuditLogDto>>('/api/admin/audit-logs', { params })).data,
    placeholderData: keepPreviousData,
  });
}
