import { Link } from 'react-router-dom';
import { PageState } from '@/shared/components/PageState';
import { cn } from '@/shared/utils/cn';
import { useDashboardActions, type DashboardActionsDto } from './dashboardApi';

interface Tile {
  key: keyof DashboardActionsDto;
  label: string;
  hint: string;
  /** A page in the app, or "#…" for a panel further down the dashboard. */
  to: string;
}

/**
 * The five kinds of work waiting for the manager, and where each one is done. The first three open their tab
 * in the "Trips that need you" list just below the tiles.
 */
const ACTION_TILES: Tile[] = [
  {
    key: 'acceptedToConfirm',
    label: 'Accepted — confirm',
    hint: 'Confirm to book the trip',
    to: '/dashboard?attention=ClientAccepted',
  },
  {
    key: 'declinedNeedsDecision',
    label: 'Declined — needs a decision',
    hint: 'Replan with a note, or cancel',
    to: '/dashboard?attention=ClientDeclined',
  },
  {
    key: 'needsOperator',
    label: 'Needs operator',
    hint: 'The agents could not finish',
    to: '/dashboard?attention=NeedsOperator',
  },
  {
    key: 'guideChangeRequests',
    label: 'Guide change requests',
    hint: 'Pick a replacement guide',
    to: '#guide-change-requests',
  },
  {
    key: 'recentCancellations',
    label: 'Cancellations (7 days)',
    hint: 'Trips cancelled this week',
    to: '/trips?status=Cancelled',
  },
];

/** "Needs your action": one clickable count per kind of work (GET /api/dashboard/actions). */
export function ActionTiles() {
  const actions = useDashboardActions();
  return (
    <section aria-labelledby="needs-action" className="space-y-3">
      <h2 id="needs-action" className="text-lg font-semibold text-slate-900">
        Needs your action
      </h2>
      <PageState
        isLoading={actions.isLoading}
        isError={actions.isError}
        error={actions.error}
        onRetry={() => actions.refetch()}
      >
        {actions.data && (
          <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-5">
            {ACTION_TILES.map((tile) => (
              <li key={tile.key}>
                <ActionTile tile={tile} count={actions.data[tile.key]} />
              </li>
            ))}
          </ul>
        )}
      </PageState>
    </section>
  );
}

function ActionTile({ tile, count }: { tile: Tile; count: number }) {
  const className = cn(
    'card flex h-full flex-col gap-1 transition-colors hover:border-brand-600 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
    count > 0 && 'border-amber-300',
  );
  const content = (
    <>
      <span className="text-3xl font-semibold tabular-nums text-slate-900">{count}</span>
      <span className="font-medium text-slate-900">{tile.label}</span>
      <span className="text-xs text-slate-500">{count > 0 ? tile.hint : 'Nothing waiting'}</span>
    </>
  );
  const name = `${tile.label}: ${count}`;
  // A "#…" target is a panel on this page, so a plain in-page link is used.
  return tile.to.startsWith('#') ? (
    <a href={tile.to} aria-label={name} className={className}>
      {content}
    </a>
  ) : (
    <Link to={tile.to} aria-label={name} className={className}>
      {content}
    </Link>
  );
}
