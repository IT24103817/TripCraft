import { cn } from '@/shared/utils/cn';
import { AvailabilityCell } from './AvailabilityCell';
import { CELL_LOOK } from './availabilityCells';
import { formatDayMonth, formatWeekday } from './availabilityDates';
import type {
  AvailabilityCellDto,
  AvailabilityGridDto,
  AvailabilityRowDto,
  CellState,
  ResourceType,
} from './types';

const GROUPS: { type: ResourceType; label: string }[] = [
  { type: 'Guide', label: 'Guides' },
  { type: 'Vehicle', label: 'Vehicles' },
  { type: 'Room', label: 'Hotel room types' },
];

const STATES: CellState[] = ['Free', 'Held', 'Confirmed', 'Blocked'];

interface Props {
  grid: AvailabilityGridDto;
  onOpen: (row: AvailabilityRowDto, cell: AvailabilityCellDto) => void;
}

/** Resources down the side (grouped: guides, vehicles, room types), days across the top. */
export function AvailabilityGrid({ grid, onOpen }: Props) {
  return (
    <div className="card space-y-3 p-0">
      <ul aria-label="Legend" className="flex flex-wrap gap-4 px-4 pt-4 text-xs text-slate-700">
        {STATES.map((state) => (
          <li key={state} className="flex items-center gap-2">
            <span
              aria-hidden="true"
              className={cn(
                'flex h-6 w-6 items-center justify-center rounded-md font-semibold ring-1 ring-inset',
                CELL_LOOK[state].className,
              )}
            >
              {CELL_LOOK[state].symbol}
            </span>
            {CELL_LOOK[state].label}
          </li>
        ))}
        <li>Room types show the number of rooms still free.</li>
      </ul>
      <div className="overflow-x-auto pb-24">
        <table aria-label="Availability grid" className="min-w-full border-separate border-spacing-1 text-sm">
          <thead>
            <tr>
              <th scope="col" className="sticky left-0 z-10 bg-surface px-2 text-left text-xs text-slate-500">
                Resource
              </th>
              {grid.days.map((day) => (
                <th key={day} scope="col" className="px-1 text-center text-xs font-medium text-slate-500">
                  <span className="block">{formatWeekday(day)}</span>
                  <span className="block text-slate-900">{formatDayMonth(day)}</span>
                </th>
              ))}
            </tr>
          </thead>
          {GROUPS.map((group) => {
            const rows = grid.rows.filter((row) => row.resourceType === group.type);
            if (rows.length === 0) return null;
            return (
              <tbody key={group.type}>
                <tr>
                  <th
                    scope="colgroup"
                    colSpan={grid.days.length + 1}
                    className="sticky left-0 bg-slate-50 px-2 py-1 text-left text-xs font-semibold uppercase tracking-wide text-slate-500"
                  >
                    {group.label}
                  </th>
                </tr>
                {rows.map((row) => (
                  <tr key={row.resourceId}>
                    <th
                      scope="row"
                      className="sticky left-0 z-10 whitespace-nowrap bg-surface px-2 text-left font-medium text-slate-900"
                    >
                      {row.name}
                      <span className="block text-xs font-normal text-slate-500">{row.detail}</span>
                    </th>
                    {row.cells.map((cell) => (
                      <td key={cell.date} className="p-0">
                        <AvailabilityCell row={row} cell={cell} onOpen={onOpen} />
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            );
          })}
        </table>
      </div>
    </div>
  );
}
