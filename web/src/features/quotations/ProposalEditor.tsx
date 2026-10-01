import { useCallback, useState } from 'react';
import { ProposalDayEditor } from './ProposalDayEditor';
import { ResourceSwapForm } from './ResourceSwapForm';
import type { ProposalDay, ProposalResources, TripSummary } from './types';

interface Props {
  trip: TripSummary;
  days: ProposalDay[];
  resources: ProposalResources | null;
  names: Record<string, string>;
}

/**
 * "Edit directly" (trip PendingReview): change a day's stops or swap the guide, vehicle or hotel. After any
 * edit "Send to client" stays disabled until Re-price has made a new version.
 */
export function ProposalEditor({ trip, days, resources, names }: Props) {
  const [editingDay, setEditingDay] = useState<ProposalDay | null>(null);
  // Stable, so the open dialog does not move focus again when the page re-renders.
  const closeEditor = useCallback(() => setEditingDay(null), []);

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <div>
        <h3 className="mb-2 font-medium text-slate-900">Days</h3>
        <ul className="space-y-2 text-sm">
          {days.map((day) => (
            <li
              key={day.day}
              className="flex flex-wrap items-center justify-between gap-2 rounded border border-slate-200 p-2"
            >
              <span>
                <span className="font-medium text-slate-900">
                  Day {day.day} — {day.city}:
                </span>{' '}
                {(day.stops ?? []).map((s) => s.name).join(', ') || 'no stops'}
              </span>
              <button
                type="button"
                className="btn-secondary"
                aria-label={`Edit day ${day.day}`}
                onClick={() => setEditingDay(day)}
              >
                Edit day
              </button>
            </li>
          ))}
        </ul>
      </div>
      <div>
        <h3 className="mb-2 font-medium text-slate-900">Guide, vehicle and hotels</h3>
        <ResourceSwapForm trip={trip} days={days} resources={resources} names={names} />
      </div>
      {editingDay && <ProposalDayEditor tripId={trip.id} day={editingDay} onClose={closeEditor} />}
    </div>
  );
}
