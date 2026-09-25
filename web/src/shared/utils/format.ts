const dateFormat = new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
const dateTimeFormat = new Intl.DateTimeFormat('en-GB', {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

/** "2026-10-10" (DateOnly) or an ISO date-time -> "10 Oct 2026". */
export function formatDate(value: string | null | undefined): string {
  if (!value) return '—';
  const date = value.length === 10 ? new Date(`${value}T00:00:00`) : new Date(value);
  return Number.isNaN(date.getTime()) ? value : dateFormat.format(date);
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : dateTimeFormat.format(date);
}

export function formatLkr(amount: number): string {
  return `LKR ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

export function formatUsd(amount: number): string {
  return `USD ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

export function formatDuration(ms: number | null | undefined): string {
  if (ms == null) return '—';
  if (ms < 1000) return `${ms} ms`;
  const seconds = ms / 1000;
  return seconds < 60
    ? `${seconds.toFixed(1)} s`
    : `${Math.floor(seconds / 60)} min ${Math.round(seconds % 60)} s`;
}

/** First 8 characters of a Guid, for compact tables. */
export function shortId(id: string): string {
  return id.slice(0, 8);
}

/** yyyy-MM-dd for DateOnly query parameters, in local time. */
export function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

export function monthRange(date = new Date()): { from: string; to: string } {
  return {
    from: toIsoDate(new Date(date.getFullYear(), date.getMonth(), 1)),
    to: toIsoDate(new Date(date.getFullYear(), date.getMonth() + 1, 0)),
  };
}
