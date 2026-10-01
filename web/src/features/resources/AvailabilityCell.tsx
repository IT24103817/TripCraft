import { useId, useState } from 'react';
import { cn } from '@/shared/utils/cn';
import { CELL_LOOK, cellLabel, cellSummary, cellText, freeText } from './availabilityCells';
import { formatDayMonth } from './availabilityDates';
import type { AvailabilityCellDto, AvailabilityRowDto } from './types';

interface Props {
  row: AvailabilityRowDto;
  cell: AvailabilityCellDto;
  onOpen: (row: AvailabilityRowDto, cell: AvailabilityCellDto) => void;
}

/**
 * One resource on one day: a button (so the grid works with the keyboard) coloured by its state, with a symbol
 * and a full description as its name. Hover or focus shows a tooltip; a click opens the side panel.
 */
export function AvailabilityCell({ row, cell, onOpen }: Props) {
  const [showTip, setShowTip] = useState(false);
  const tipId = useId();
  const look = CELL_LOOK[cell.state];
  const free = freeText(row, cell);

  return (
    <div className="relative">
      <button
        type="button"
        aria-label={cellLabel(row, cell)}
        aria-describedby={showTip ? tipId : undefined}
        className={cn(
          'flex h-11 w-full min-w-11 items-center justify-center rounded-md text-xs font-semibold ring-1 ring-inset focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
          look.className,
        )}
        onMouseEnter={() => setShowTip(true)}
        onMouseLeave={() => setShowTip(false)}
        onFocus={() => setShowTip(true)}
        onBlur={() => setShowTip(false)}
        onClick={() => onOpen(row, cell)}
      >
        <span aria-hidden="true">{cellText(row, cell)}</span>
      </button>
      {showTip && (
        <div
          role="tooltip"
          id={tipId}
          className="absolute left-1/2 top-full z-20 mt-1 w-56 -translate-x-1/2 rounded-md border border-slate-600 bg-ink p-2 text-left text-xs font-normal text-white shadow-lg"
        >
          <p className="font-semibold">
            {row.name} · {formatDayMonth(cell.date)}
          </p>
          <p>{cellSummary(cell)}</p>
          {free && <p>{free}</p>}
        </div>
      )}
    </div>
  );
}
