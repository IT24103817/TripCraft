import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { useRevenue } from '@/features/quotations/quotationsApi';
import { PageState } from '@/shared/components/PageState';
import { CHART_COLORS } from '@/shared/theme';
import { formatUsd, lastMonths, monthRange } from '@/shared/utils/format';

const MONTHS = 6;

/**
 * Revenue (quotations sent to clients, USD) for the last six months from GET /api/reports/revenue. A month with
 * nothing sent shows 0. Screen readers get the same numbers as a table.
 */
export function RevenueChart() {
  const months = lastMonths(MONTHS);
  // From the first day of the oldest month to the last day of this month.
  const from = `${months[0]}-01`;
  const to = monthRange().to;
  const revenue = useRevenue(from, to);
  const data = months.map((month) => ({
    name: month,
    value: revenue.data?.find((m) => m.month === month)?.totalUsd ?? 0,
  }));

  return (
    <section aria-labelledby="revenue-chart" className="card space-y-3">
      <h2 id="revenue-chart" className="font-semibold text-slate-900">
        Revenue, last 6 months
      </h2>
      <PageState
        isLoading={revenue.isLoading}
        isError={revenue.isError}
        error={revenue.error}
        onRetry={() => revenue.refetch()}
      >
        <div className="h-56" aria-hidden="true">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={data} margin={{ top: 8, right: 8, bottom: 8, left: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={CHART_COLORS.grid} />
              <XAxis dataKey="name" fontSize={11} />
              <YAxis fontSize={11} />
              <Tooltip formatter={(v: number) => formatUsd(v)} />
              <Bar dataKey="value" fill={CHART_COLORS.primary} name="Revenue (USD)" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
        <table className="sr-only">
          <caption>Revenue for the last 6 months, in USD</caption>
          <tbody>
            {data.map((d) => (
              <tr key={d.name}>
                <th scope="row">{d.name}</th>
                <td>{formatUsd(d.value)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </PageState>
    </section>
  );
}
