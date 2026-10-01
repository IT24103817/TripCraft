import { useCoveredCities } from './api';

export interface GridFilters {
  type: string;
  language: string;
  seats: string;
  city: string;
}

interface Props {
  filters: GridFilters;
  onChange: (patch: Partial<GridFilters>) => void;
}

/** Resource type, plus one filter per kind: language for guides, minimum seats for vehicles, city for rooms. */
export function AvailabilityFilters({ filters, onChange }: Props) {
  const cities = useCoveredCities();
  return (
    <div role="search" aria-label="Availability filters" className="card flex flex-wrap items-end gap-3">
      <label className="flex flex-col gap-1 text-sm text-slate-700">
        Resource type
        <select className="input" value={filters.type} onChange={(e) => onChange({ type: e.target.value })}>
          <option value="">All</option>
          <option value="Guide">Guides</option>
          <option value="Vehicle">Vehicles</option>
          <option value="Room">Hotel room types</option>
        </select>
      </label>
      <label className="flex flex-col gap-1 text-sm text-slate-700">
        Language (guides)
        <input
          className="input w-28"
          value={filters.language}
          maxLength={2}
          placeholder="e.g. en"
          onChange={(e) => onChange({ language: e.target.value.toLowerCase() })}
        />
      </label>
      <label className="flex flex-col gap-1 text-sm text-slate-700">
        Min seats (vehicles)
        <input
          type="number"
          min={1}
          className="input w-28"
          value={filters.seats}
          onChange={(e) => onChange({ seats: e.target.value })}
        />
      </label>
      <label className="flex flex-col gap-1 text-sm text-slate-700">
        City (room types)
        <select className="input" value={filters.city} onChange={(e) => onChange({ city: e.target.value })}>
          <option value="">All</option>
          {(cities.data ?? []).map((city) => (
            <option key={city} value={city}>
              {city}
            </option>
          ))}
        </select>
      </label>
    </div>
  );
}
