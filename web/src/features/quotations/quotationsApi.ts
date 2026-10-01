import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  ProposalQuotation,
  QuotationDto,
  RepriceResponse,
  RevenueMonthDto,
  StatusCountDto,
  UtilisationDto,
} from './types';

const QUOTATIONS = queryRoots.quotations;

/**
 * GET /api/quotations with status, created from/to and minTotalUsd filters, search (trip objective),
 * sort and paging. Empty values are left out so the API only sees filters that are set.
 */
export function useQuotations(query: Record<string, string | number>) {
  const params = Object.fromEntries(Object.entries(query).filter(([, v]) => v !== ''));
  return useQuery({
    queryKey: [QUOTATIONS, 'list', params],
    queryFn: async () => (await http.get<PagedResult<QuotationDto>>('/api/quotations', { params })).data,
    placeholderData: keepPreviousData,
  });
}

export function useQuotation(id: string | null | undefined) {
  return useQuery({
    queryKey: [QUOTATIONS, 'detail', id],
    queryFn: async () => (await http.get<QuotationDto>(`/api/quotations/${id}`)).data,
    enabled: Boolean(id),
  });
}

/**
 * Every version of one trip's quotation, oldest first (GET /api/quotations?tripRequestId=&sort=version), for
 * the side-by-side comparison on the review page.
 */
export function useQuotationVersions(tripRequestId: string | undefined) {
  return useQuery({
    queryKey: [QUOTATIONS, 'versions', tripRequestId],
    queryFn: async () =>
      (
        await http.get<PagedResult<QuotationDto>>('/api/quotations', {
          params: { tripRequestId, sort: 'version', pageSize: 100 },
        })
      ).data.items,
    enabled: Boolean(tripRequestId),
  });
}

/**
 * POST /api/quotations/{id}/calculate: re-price with today's rate card and exchange rate. This makes a NEW
 * version (the old one becomes Superseded) and the workflow then points at it, so workflows are refreshed too.
 */
export function useRecalculate() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) =>
      (await http.post<RepriceResponse>(`/api/quotations/${id}/calculate`)).data,
    onSuccess: () => {
      void client.invalidateQueries({ queryKey: [QUOTATIONS] });
      void client.invalidateQueries({ queryKey: [queryRoots.workflows] });
    },
  });
}

/** The stored quotation in the shape QuotationPanel draws (the agent's snake_case proposal shape). */
export function toPanelQuotation(q: QuotationDto): ProposalQuotation {
  return {
    lines: q.lines.map((l) => ({
      line_type: l.lineType,
      description: l.description,
      qty: l.qty,
      unit_lkr: l.unitLkr,
      amount_lkr: l.amountLkr,
    })),
    subtotal_lkr: q.subtotalLkr,
    margin_pct: q.marginPct,
    margin_lkr: q.marginLkr,
    total_lkr: q.totalLkr,
    fx_rate: q.fxRate,
    fx_as_of: q.fxAsOf,
    fx_stale: q.fxStale,
    total_usd: q.totalUsd,
  };
}

function useReport<T>(path: string, from: string, to: string) {
  return useQuery({
    queryKey: [queryRoots.reports, path, from, to],
    queryFn: async () => (await http.get<T>(`/api/reports/${path}`, { params: { from, to } })).data,
    enabled: Boolean(from && to),
  });
}

export const useRevenue = (from: string, to: string) => useReport<RevenueMonthDto[]>('revenue', from, to);
export const useUtilisation = (from: string, to: string) =>
  useReport<UtilisationDto[]>('utilisation', from, to);
export const useTripsByStatus = (from: string, to: string) =>
  useReport<StatusCountDto[]>('trips-by-status', from, to);
