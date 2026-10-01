import { useCities } from './api';

interface Props {
  /** The ticked cities. */
  value: string[];
  onChange: (cities: string[]) => void;
}

/**
 * Multi-select of the cities from GET /api/attractions/cities. A trip matches when it visits every ticked city
 * (AND), so ticking more cities narrows the list.
 */
export function CityFilter({ value, onChange }: Props) {
  const cities = useCities();

  const toggle = (city: string) =>
    onChange(value.includes(city) ? value.filter((c) => c !== city) : [...value, city]);

  return (
    <fieldset className="flex flex-col gap-1 text-sm text-slate-700">
      <legend className="mb-1">Cities (all of)</legend>
      {cities.isError ? (
        <p className="text-xs text-red-700">Could not load the cities.</p>
      ) : (
        <div className="flex flex-wrap gap-x-3 gap-y-1">
          {(cities.data ?? []).map((city) => (
            <label key={city} className="flex min-h-10 items-center gap-1">
              <input
                type="checkbox"
                className="h-4 w-4"
                checked={value.includes(city)}
                onChange={() => toggle(city)}
              />
              {city}
            </label>
          ))}
        </div>
      )}
    </fieldset>
  );
}
