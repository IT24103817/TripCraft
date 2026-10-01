import { toIsoDate } from '@/shared/utils/format';

/** The availability grid shows one week (Monday to Sunday) or one calendar month. */
export type GridView = 'week' | 'month';

function parse(iso: string): Date {
  return new Date(`${iso}T00:00:00`);
}

export function addDays(iso: string, days: number): string {
  const date = parse(iso);
  date.setDate(date.getDate() + days);
  return toIsoDate(date);
}

/** The Monday of the week that holds `iso`. */
export function startOfWeek(iso: string): string {
  const date = parse(iso);
  const daysSinceMonday = (date.getDay() + 6) % 7; // getDay(): Sunday = 0, Monday = 1 …
  return addDays(iso, -daysSinceMonday);
}

export function startOfMonth(iso: string): string {
  return `${iso.slice(0, 7)}-01`;
}

/** The first day shown: the week's Monday, or the 1st of the month. */
export function viewStart(view: GridView, iso: string): string {
  return view === 'week' ? startOfWeek(iso) : startOfMonth(iso);
}

/** The first and last day shown for a view that starts on `start`. */
export function viewRange(view: GridView, start: string): { from: string; to: string } {
  if (view === 'week') return { from: start, to: addDays(start, 6) };
  const first = parse(start);
  const last = new Date(first.getFullYear(), first.getMonth() + 1, 0);
  return { from: start, to: toIsoDate(last) };
}

/** The previous (-1) or next (+1) week or month. */
export function shiftView(view: GridView, start: string, direction: 1 | -1): string {
  if (view === 'week') return addDays(start, 7 * direction);
  const first = parse(start);
  return toIsoDate(new Date(first.getFullYear(), first.getMonth() + direction, 1));
}

const dayMonth = new Intl.DateTimeFormat('en-GB', { day: 'numeric', month: 'short' });
const monthYear = new Intl.DateTimeFormat('en-GB', { month: 'long', year: 'numeric' });
const weekday = new Intl.DateTimeFormat('en-GB', { weekday: 'short' });

/** "12 Oct" */
export function formatDayMonth(iso: string): string {
  return dayMonth.format(parse(iso));
}

/** "Mon" */
export function formatWeekday(iso: string): string {
  return weekday.format(parse(iso));
}

/** "6 Oct – 12 Oct 2026" for a week, "October 2026" for a month. */
export function viewLabel(view: GridView, start: string): string {
  if (view === 'month') return monthYear.format(parse(start));
  const { to } = viewRange(view, start);
  return `${formatDayMonth(start)} – ${formatDayMonth(to)} ${to.slice(0, 4)}`;
}
