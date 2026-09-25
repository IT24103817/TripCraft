import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useListParams } from '@/shared/hooks/useListParams';
import { statusLabel } from '@/shared/statuses';
import { toIsoDate } from '@/shared/utils/format';
import { useTripStatusCounts } from './api';

/** Reports. Requests by status uses live trip counts; revenue and utilisation wait for the reports API. */
export default function ReportsPage() {
  const list = useListParams();
  const year = new Date().getFullYear();
  const from = list.get('from') || toIsoDate(new Date(year, 0, 1));
  const to = list.get('to') || toIsoDate(new Date(year, 11, 31));
  const counts = useTripStatusCounts(from, to);
  const chartData = counts.data?.map((c) => ({ name: statusLabel(c.status), count: c.count })) ?? [];

  return (
    <section className="space-y-4">
      <PageHeader title="Reports" />
      <SearchFilterBar
        dateRange={{ label: 'Trip start date', from, to, onChange: (f, t) => list.set({ from: f, to: t }) }}
      />

      <div className="card">
        <h2 className="mb-3 font-semibold text-slate-900">Trip requests by status</h2>
        <PageState
          isLoading={counts.isLoading}
          isError={counts.isError}
          error={counts.error}
          onRetry={counts.refetch}
          isEmpty={chartData.every((d) => d.count === 0)}
          emptyTitle="No trip requests start in this period"
        >
          <div className="h-72" aria-hidden="true">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} margin={{ top: 8, right: 8, bottom: 40, left: 0 }}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" angle={-30} textAnchor="end" interval={0} fontSize={11} />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="count" fill="#4f46e5" name="Requests" />
              </BarChart>
            </ResponsiveContainer>
          </div>
          <table className="sr-only">
            <caption>Trip requests by status</caption>
            <tbody>
              {chartData.map((d) => (
                <tr key={d.name}>
                  <th scope="row">{d.name}</th>
                  <td>{d.count}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </PageState>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <PendingChart title="Revenue by month" api="GET /api/reports/revenue?from=&to=" />
        <PendingChart title="Guide utilisation" api="GET /api/reports/utilisation" />
      </div>
    </section>
  );
}

function PendingChart({ title, api }: { title: string; api: string }) {
  return (
    <div className="card border-dashed text-sm text-slate-700">
      <h2 className="mb-2 font-semibold text-slate-900">{title}</h2>
      <p>Available when Quotation, Approval &amp; Reporting (Student C) is merged. Needs:</p>
      <p className="mt-1 font-mono text-xs">{api}</p>
    </div>
  );
}
