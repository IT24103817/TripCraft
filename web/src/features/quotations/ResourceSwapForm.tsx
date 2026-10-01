import { useId, useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { PageState } from '@/shared/components/PageState';
import { useToast } from '@/shared/components/Toast';
import { formatLkr } from '@/shared/utils/format';
import { useAvailableOptions, useSwapResources } from './reviewApi';
import type { ProposalDay, ProposalResources, SwapResourcesRequest, TripSummary } from './types';

interface Props {
  trip: TripSummary;
  days: ProposalDay[];
  resources: ProposalResources | null;
  /** id → display name from the workflow (resourceNames). */
  names: Record<string, string>;
}

/** The nights spent in each city (a night is a trip day before the last day), as an availability date range. */
function cityStays(days: ProposalDay[], endDate: string) {
  const stays = new Map<string, { from: string; to: string }>();
  for (const day of days) {
    if (day.date >= endDate) continue; // no hotel night on the last day
    const stay = stays.get(day.city);
    if (!stay) stays.set(day.city, { from: day.date, to: day.date });
    else stays.set(day.city, { from: stay.from, to: day.date > stay.to ? day.date : stay.to });
  }
  return [...stays.entries()].map(([city, range]) => ({ city, ...range }));
}

/**
 * "Edit directly": swap the guide, the vehicle or one city's hotel room type for one that is free on the trip's
 * dates (GET /api/availability). Each swap marks the proposal as edited, so it must be re-priced before sending.
 */
export function ResourceSwapForm({ trip, days, resources, names }: Props) {
  const language = typeof trip.preferences.language === 'string' ? trip.preferences.language : 'en';
  const dates = { from: trip.startDate, to: trip.endDate };
  const label = (id: string | null | undefined) => (id ? (names[id] ?? id) : 'none');

  return (
    <div className="space-y-4">
      <SwapRow
        tripId={trip.id}
        title="Guide"
        current={label(resources?.guide_id)}
        params={{ type: 'Guide', ...dates, language, pax: trip.pax }}
        toRequest={(guideId) => ({ guideId })}
      />
      <SwapRow
        tripId={trip.id}
        title="Vehicle"
        current={label(resources?.vehicle_id)}
        params={{ type: 'Vehicle', ...dates, seats: trip.pax }}
        toRequest={(vehicleId) => ({ vehicleId })}
      />
      {cityStays(days, trip.endDate).map((stay) => {
        const nights = days.filter((d) => d.city === stay.city).map((d) => d.date);
        const roomTypes = new Set(
          (resources?.rooms ?? []).filter((r) => nights.includes(r.night)).map((r) => r.room_type_id),
        );
        return (
          <SwapRow
            key={stay.city}
            tripId={trip.id}
            title={`Hotel room in ${stay.city}`}
            current={[...roomTypes].map(label).join(', ') || 'none'}
            // One free room per night is enough to list a room type; the API checks the party fits.
            params={{ type: 'Room', city: stay.city, from: stay.from, to: stay.to, rooms: 1 }}
            toRequest={(roomTypeId) => ({ rooms: [{ city: stay.city, roomTypeId }] })}
          />
        );
      })}
    </div>
  );
}

interface SwapRowProps {
  tripId: string;
  title: string;
  current: string;
  params: Record<string, string | number>;
  toRequest: (id: string) => SwapResourcesRequest;
}

function SwapRow({ tripId, title, current, params, toRequest }: SwapRowProps) {
  const toast = useToast();
  const selectId = useId();
  const options = useAvailableOptions(params);
  const swap = useSwapResources(tripId);
  const [choice, setChoice] = useState('');

  const submit = () =>
    swap.mutate(toRequest(choice), {
      onSuccess: () => {
        toast.success(`${title} swapped. Re-price before sending it to the client.`);
        setChoice('');
      },
      onError: (error) => toast.error(getErrorMessage(error)),
    });

  return (
    <div className="space-y-1 text-sm">
      <p className="text-slate-700">
        <span className="font-medium text-slate-900">{title}</span> — now: {current}
      </p>
      <PageState
        isLoading={options.isLoading}
        isError={options.isError}
        error={options.error}
        onRetry={() => options.refetch()}
        isEmpty={options.data?.length === 0}
        emptyTitle="Nothing else is free on these dates"
      >
        <div className="flex flex-wrap items-end gap-2">
          <label htmlFor={selectId} className="flex flex-col gap-1 text-slate-700">
            Swap {title.toLowerCase()} for
            <select
              id={selectId}
              className="input"
              value={choice}
              onChange={(e) => setChoice(e.target.value)}
            >
              <option value="">Choose…</option>
              {options.data?.map((option) => (
                <option key={option.id} value={option.id}>
                  {option.name} · {option.detail} · {formatLkr(option.rateLkr)}
                </option>
              ))}
            </select>
          </label>
          <button
            type="button"
            className="btn-secondary"
            disabled={choice === '' || swap.isPending}
            onClick={submit}
          >
            {swap.isPending ? 'Swapping…' : 'Swap'}
          </button>
        </div>
      </PageState>
    </div>
  );
}
