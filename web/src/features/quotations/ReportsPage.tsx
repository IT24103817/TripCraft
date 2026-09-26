import type { ReactNode } from 'react';
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useListParams } from '@/shared/hooks/useListParams';
import { statusLabel } from '@/shared/statuses';
import { formatLkr, formatUsd, toIsoDate } from '@/shared/utils/format';
import { useRevenue, useTripsByStatus, useUtilisation } from './quotationsApi';

/** Component C reporting: trip requests by status, revenue by month and guide/vehicle utilisation for a date range. */
export default function ReportsPage() {
  const list = useListParams();
  const year = new Date().getFullYear();
  const from = list.get('from') || toIsoDate(new Date(year, 0, 1));
  const to = list.get('to') || toIsoDate(new Date(year, 11, 31));
  const statuses = useTripsByStatus(from, to);
  const revenue = useRevenue(from, to);
  const utilisation = useUtilisation(from, to);

  const statusData = statuses.data?.map((s) => ({ name: statusLabel(s.status), value: s.count })) ?? [];
  const revenueData = revenue.data?.map((m) => ({ name: m.month, value: m.totalUsd, lkr: m.totalLkr })) ?? [];
  const utilisationData =
    utilisation.data?.map((u) => ({ name: `${u.name} (${u.resourceType})`, value: u.utilisationPct })) ?? [];

  return (
    <section className="space-y-4">
      <PageHeader title="Reports" description="Every figure is calculated by the API from the database." />
      <SearchFilterBar
        dateRange={{ label: 'Period', from, to, onChange: (f, t) => list.set({ from: f, to: t }) }}
      />

      <ReportCard
        title="Trip requests by status"
        caption="Trip requests starting in the period, per status"
        query={statuses}
        data={statusData}
        format={(v) => String(v)}
        empty="No trip requests start in this period"
      />
      <div className="grid gap-4 lg:grid-cols-2">
        <ReportCard
          title="Revenue by month"
          caption="Approved quotations per month, in USD"
          query={revenue}
          data={revenueData}
          format={formatUsd}
          empty="No quotations were approved in this period"
          footer={
            revenue.data && revenue.data.length > 0 ? (
              <p className="text-sm text-slate-600">
                Total {formatLkr(revenue.data.reduce((n, m) => n + m.totalLkr, 0))} from{' '}
                {revenue.data.reduce((n, m) => n + m.quotations, 0)} approved quotations.
              </p>
            ) : null
          }
        />
        <ReportCard
          title="Guide and vehicle utilisation"
          caption="Held days as a percentage of the days in the period"
          query={utilisation}
          data={utilisationData}
          format={(v) => `${v}%`}
          empty="No active guides or vehicles"
        />
      </div>
    </section>
  );
}

interface ReportQuery {
  isLoading: boolean;
  isError: boolean;
  error: unknown;
  refetch: () => unknown;
}

interface ReportCardProps {
  title: string;
  caption: string;
  query: ReportQuery;
  data: { name: string; value: number }[];
  format: (value: number) => string;
  empty: string;
  footer?: ReactNode;
}

/** A bar chart for sighted users plus the same numbers as a table for screen readers. */
function ReportCard({ title, caption, query, data, format, empty, footer }: ReportCardProps) {
  return (
    <div className="card space-y-3">
      <h2 className="font-semibold text-slate-900">{title}</h2>
      <PageState
        isLoading={query.isLoading}
        isError={query.isError}
        error={query.error}
        onRetry={() => query.refetch()}
        isEmpty={data.length === 0 || data.every((d) => d.value === 0)}
        emptyTitle={empty}
      >
        <div className="h-72" aria-hidden="true">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={data} margin={{ top: 8, right: 8, bottom: 48, left: 0 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" angle={-30} textAnchor="end" interval={0} fontSize={11} />
              <YAxis />
              <Tooltip formatter={(v: number) => format(v)} />
              <Bar dataKey="value" fill="#4f46e5" name={title} />
            </BarChart>
          </ResponsiveContainer>
        </div>
        <table className="sr-only">
          <caption>{caption}</caption>
          <tbody>
            {data.map((d) => (
              <tr key={d.name}>
                <th scope="row">{d.name}</th>
                <td>{format(d.value)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {footer}
      </PageState>
    </div>
  );
}
