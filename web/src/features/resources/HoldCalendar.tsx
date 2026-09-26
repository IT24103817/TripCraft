import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { PageState } from '@/shared/components/PageState';
import { useToast } from '@/shared/components/Toast';
import { formatDate, toIsoDate } from '@/shared/utils/format';
import { useHolds, useReleaseHold } from './api';
import type { HoldDto } from './types';

const DAYS = 14;

function addDays(iso: string, days: number): string {
  const date = new Date(`${iso}T00:00:00`);
  date.setDate(date.getDate() + days);
  return toIsoDate(date);
}

/** 14-day grid of Held holds: one row per resource, a filled cell for each held day. Holds can be released. */
export function HoldCalendar() {
  const toast = useToast();
  const release = useReleaseHold();
  const [start, setStart] = useState(toIsoDate(new Date()));
  const end = addDays(start, DAYS - 1);
  const holds = useHolds(start, end);
  const days = Array.from({ length: DAYS }, (_, i) => addDays(start, i));

  const rows = new Map<string, HoldDto[]>();
  for (const hold of holds.data?.items ?? []) {
    const key = `${hold.resourceType}: ${hold.resourceName}`;
    rows.set(key, [...(rows.get(key) ?? []), hold]);
  }
  const heldOn = (list: HoldDto[], day: string) => list.find((h) => h.fromDate <= day && day <= h.toDate);

  return (
    <div className="card space-y-3">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <h2 className="font-semibold text-slate-900">Hold calendar</h2>
        <label className="flex items-center gap-2 text-sm">
          Starting
          <input
            className="input"
            type="date"
            value={start}
            onChange={(e) => setStart(e.target.value || start)}
          />
        </label>
      </div>
      <PageState
        isLoading={holds.isLoading}
        isError={holds.isError}
        error={holds.error}
        onRetry={() => holds.refetch()}
        isEmpty={holds.data?.items.length === 0}
        emptyTitle="Nothing is held in these two weeks"
      >
        <div className="overflow-x-auto">
          <table className="min-w-full text-xs" aria-label="Hold calendar">
            <thead>
              <tr>
                <th className="px-2 py-1 text-left">Resource</th>
                {days.map((d) => (
                  <th key={d} className="px-1 py-1 font-normal text-slate-500">
                    {d.slice(8)}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {[...rows.entries()].map(([resource, list]) => (
                <tr key={resource} className="border-t border-slate-100">
                  <th
                    scope="row"
                    className="whitespace-nowrap px-2 py-1 text-left font-medium text-slate-800"
                  >
                    {resource}
                  </th>
                  {days.map((d) => {
                    const hold = heldOn(list, d);
                    return (
                      <td key={d} className="px-1 py-1">
                        <span
                          className={`block h-4 w-full rounded ${hold ? (hold.tripRequestId ? 'bg-indigo-500' : 'bg-amber-400') : 'bg-slate-100'}`}
                          title={
                            hold
                              ? `${hold.tripRequestId ? 'Trip' : (hold.note ?? 'Manual block')} · ${hold.quantity}`
                              : 'Free'
                          }
                        />
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <ul aria-label="Holds in this window" className="divide-y divide-slate-200 text-sm">
          {holds.data?.items.map((h) => (
            <li key={h.id} className="flex flex-wrap items-center justify-between gap-2 py-2">
              <span>
                {h.resourceName} · {formatDate(h.fromDate)} – {formatDate(h.toDate)} ·{' '}
                {h.tripRequestId ? 'trip' : (h.note ?? 'manual block')}
              </span>
              <button
                type="button"
                className="text-red-700 hover:underline"
                aria-label={`Release ${h.resourceName}`}
                onClick={() =>
                  release.mutate(h.id, {
                    onSuccess: () => toast.success(`Released ${h.resourceName}.`),
                    onError: (error) => toast.error(getErrorMessage(error)),
                  })
                }
              >
                Release
              </button>
            </li>
          ))}
        </ul>
      </PageState>
    </div>
  );
}
