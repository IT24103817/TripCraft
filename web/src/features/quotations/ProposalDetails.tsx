import { formatDate, formatLkr } from '@/shared/utils/format';
import type { ProposalDay, ProposalResources } from './types';

export function ProposedItinerary({ days }: { days: ProposalDay[] }) {
  return (
    <ol className="space-y-3">
      {days.map((day) => (
        <li key={day.day} className="rounded border border-slate-200 p-3 text-sm">
          <h3 className="font-medium text-slate-900">
            Day {day.day} — {day.city}{' '}
            <span className="font-normal text-slate-500">({formatDate(day.date)})</span>
          </h3>
          <p className="text-slate-600">
            Travel: {day.transport}
            {day.transfer_km > 0 && `, ${day.transfer_km} km`}
            {day.driving_minutes > 0 && `, ${day.driving_minutes} min driving`}
            {day.weather && ` · Weather: ${day.weather}`}
          </p>
          <ul className="mt-1 list-inside list-disc text-slate-700">
            {(day.stops ?? []).map((stop, i) => (
              <li key={`${stop.attraction_id}-${i}`}>
                {stop.name}
                {stop.entry_fee_lkr > 0 && (
                  <span className="text-slate-500"> — entry {formatLkr(stop.entry_fee_lkr)} pp</span>
                )}
              </li>
            ))}
          </ul>
        </li>
      ))}
    </ol>
  );
}

/** Guide, vehicle and rooms with the names the API sends (an id without a name is shown as it is). */
export function ProposedResources({
  resources,
  names = {},
}: {
  resources: ProposalResources;
  /** id → display name from the API; an id without a name is shown as it is. */
  names?: Record<string, string>;
}) {
  const label = (id: string) => names[id] ?? id;
  const nights = new Map<string, Map<string, number>>();
  for (const room of resources.rooms ?? []) {
    const byType = nights.get(room.night) ?? new Map<string, number>();
    byType.set(room.room_type_id, (byType.get(room.room_type_id) ?? 0) + 1);
    nights.set(room.night, byType);
  }
  return (
    <dl className="space-y-2 text-sm">
      <div>
        <dt className="text-slate-500">Guide</dt>
        <dd className="text-slate-900">{resources.guide_id ? label(resources.guide_id) : 'none proposed'}</dd>
      </div>
      <div>
        <dt className="text-slate-500">Vehicle</dt>
        <dd className="text-slate-900">
          {resources.vehicle_id ? label(resources.vehicle_id) : 'none proposed'}
        </dd>
      </div>
      <div>
        <dt className="text-slate-500">Rooms</dt>
        <dd>
          <ul className="text-slate-900">
            {[...nights.entries()].map(([night, byType]) => (
              <li key={night}>
                {formatDate(night)}:{' '}
                {[...byType.entries()].map(([type, count]) => (
                  <span key={type}>
                    {count} × {label(type)}{' '}
                  </span>
                ))}
              </li>
            ))}
          </ul>
        </dd>
      </div>
      {(resources.gaps ?? []).length > 0 && (
        <div>
          <dt className="text-slate-500">Gaps reported by the agent</dt>
          <dd>
            <ul className="list-inside list-disc text-amber-800">
              {resources.gaps?.map((gap) => (
                <li key={gap}>{gap}</li>
              ))}
            </ul>
          </dd>
        </div>
      )}
    </dl>
  );
}
