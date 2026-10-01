import { statusLabel } from '@/shared/statuses';
import { formatDayMonth } from './availabilityDates';
import type { AvailabilityCellDto, AvailabilityRowDto, CellState } from './types';

/**
 * How each state looks. Colour follows the status tones (free = plain surface, held = warning, confirmed =
 * success, blocked = danger) and every state also has a symbol and a word, so colour is never the only cue.
 */
export const CELL_LOOK: Record<CellState, { className: string; symbol: string; label: string }> = {
  Free: { className: 'bg-surface text-slate-600 ring-slate-200', symbol: '', label: 'Free' },
  Held: { className: 'bg-amber-50 text-amber-800 ring-amber-300', symbol: '◐', label: 'Held' },
  Confirmed: { className: 'bg-green-50 text-green-700 ring-green-300', symbol: '✓', label: 'Confirmed' },
  Blocked: { className: 'bg-red-50 text-red-700 ring-red-300', symbol: '✕', label: 'Blocked' },
};

/** What the cell shows: its symbol, and for room types the number of rooms still free. */
export function cellText(row: AvailabilityRowDto, cell: AvailabilityCellDto): string {
  const symbol = CELL_LOOK[cell.state].symbol;
  return row.resourceType === 'Room' ? `${symbol}${cell.freeQuantity}` : symbol;
}

/** "Held for trip of Anna Silva (Quotation sent)", "Blocked: Annual leave" or "Free". */
export function cellSummary(cell: AvailabilityCellDto): string {
  if (cell.state === 'Free') return 'Free';
  if (cell.state === 'Blocked') return `Blocked: ${cell.note || 'manual block'}`;
  const who = cell.touristName ? `trip of ${cell.touristName}` : 'a trip';
  const status = cell.tripStatus ? ` (${statusLabel(cell.tripStatus)})` : '';
  return `${CELL_LOOK[cell.state].label} for ${who}${status}`;
}

/** "3 of 5 rooms free" for room types; guides and vehicles are either free or not. */
export function freeText(row: AvailabilityRowDto, cell: AvailabilityCellDto): string | null {
  return row.resourceType === 'Room' ? `${cell.freeQuantity} of ${row.capacity} rooms free` : null;
}

/** The button's accessible name, e.g. "Nimal Perera, 12 Oct: Held for trip of Anna Silva (Quotation sent)". */
export function cellLabel(row: AvailabilityRowDto, cell: AvailabilityCellDto): string {
  const free = freeText(row, cell);
  return `${row.name}, ${formatDayMonth(cell.date)}: ${cellSummary(cell)}${free ? `, ${free}` : ''}`;
}
