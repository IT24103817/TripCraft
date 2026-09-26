import { useState, type FormEvent } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { PageState } from '@/shared/components/PageState';
import { useToast } from '@/shared/components/Toast';
import { formatLkr, toIsoDate } from '@/shared/utils/format';
import { useAvailability, useCreateHold } from './api';
import type { AvailableResourceDto, ListQuery, ResourceType } from './types';

const today = toIsoDate(new Date());

/** GET /api/availability: which guides, vehicles or rooms are free, with the option to block one. */
export function AvailabilitySearch() {
  const toast = useToast();
  const block = useCreateHold();
  const [type, setType] = useState<ResourceType>('Vehicle');
  const [from, setFrom] = useState(today);
  const [to, setTo] = useState(today);
  const [language, setLanguage] = useState('en');
  const [amount, setAmount] = useState('4'); // pax, seats or rooms depending on the type
  const [city, setCity] = useState('Kandy');
  const [query, setQuery] = useState<ListQuery | null>(null);
  const results = useAvailability(query);

  const search = (event: FormEvent) => {
    event.preventDefault();
    const base = { type, from, to };
    setQuery(
      type === 'Guide'
        ? { ...base, language, pax: amount }
        : type === 'Vehicle'
          ? { ...base, seats: amount }
          : { ...base, city, rooms: amount },
    );
  };

  const blockResource = (r: AvailableResourceDto) =>
    block.mutate(
      {
        resourceType: r.type,
        resourceId: r.id,
        fromDate: from,
        toDate: to,
        quantity: 1,
        note: 'Blocked by manager',
      },
      {
        onSuccess: () => toast.success(`Blocked ${r.name}.`),
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    );

  const amountLabel = type === 'Guide' ? 'Group size' : type === 'Vehicle' ? 'Seats' : 'Rooms';
  return (
    <div className="card space-y-4">
      <h2 className="font-semibold text-slate-900">Find available resources</h2>
      <form
        className="grid grid-cols-2 gap-3 sm:grid-cols-6"
        onSubmit={search}
        aria-label="Availability search"
      >
        <label className="flex flex-col gap-1 text-sm">
          Type
          <select className="input" value={type} onChange={(e) => setType(e.target.value as ResourceType)}>
            <option value="Guide">Guide</option>
            <option value="Vehicle">Vehicle</option>
            <option value="Room">Room</option>
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          From
          <input className="input" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          To
          <input className="input" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        </label>
        {type === 'Guide' && (
          <label className="flex flex-col gap-1 text-sm">
            Language
            <input
              className="input"
              value={language}
              maxLength={2}
              onChange={(e) => setLanguage(e.target.value)}
            />
          </label>
        )}
        {type === 'Room' && (
          <label className="flex flex-col gap-1 text-sm">
            City
            <input className="input" value={city} onChange={(e) => setCity(e.target.value)} />
          </label>
        )}
        <label className="flex flex-col gap-1 text-sm">
          {amountLabel}
          <input
            className="input"
            type="number"
            min={1}
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
          />
        </label>
        <div className="flex items-end">
          <button type="submit" className="btn-primary w-full">
            Search
          </button>
        </div>
      </form>
      {query && (
        <PageState
          isLoading={results.isLoading}
          isError={results.isError}
          error={results.error}
          onRetry={() => results.refetch()}
          isEmpty={results.data?.length === 0}
          emptyTitle="Nothing is free for these dates"
        >
          <ul aria-label="Available resources" className="divide-y divide-slate-200 text-sm">
            {results.data?.map((r) => (
              <li key={r.id} className="flex flex-wrap items-center justify-between gap-2 py-2">
                <span>
                  <span className="font-medium text-slate-900">{r.name}</span> · {r.detail} ·{' '}
                  {formatLkr(r.rateLkr)}
                  {r.freeRooms !== null && ` · ${r.freeRooms} free`}
                </span>
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => blockResource(r)}
                  aria-label={`Block ${r.name}`}
                >
                  Block
                </button>
              </li>
            ))}
          </ul>
        </PageState>
      )}
    </div>
  );
}
