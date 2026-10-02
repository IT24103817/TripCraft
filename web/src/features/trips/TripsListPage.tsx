import type { ReactNode } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { PageHeader } from '@/shared/components/PageHeader';
import { TabList } from '@/shared/components/TabList';
import { TripRequestsList } from './TripRequestsList';

const TABS = [
  { id: 'trips', label: 'Trips' },
  { id: 'quotations', label: 'Quotations' },
];

interface Props {
  /** The Quotations tab's content; the app passes it in (features never import each other). */
  quotationsTab?: () => ReactNode;
}

/** The Trips page: a Trips tab (every trip request) and a Quotations tab (?tab=quotations). */
export default function TripsListPage({ quotationsTab }: Props) {
  const [params, setParams] = useSearchParams();
  const tab = params.get('tab') === 'quotations' && quotationsTab ? 'quotations' : 'trips';

  return (
    <section className="space-y-4">
      <PageHeader
        title="Trips"
        description="Every trip request submitted from the mobile app, through review, booking and the trip itself."
        actions={
          <Link to="/attractions" className="btn-secondary">
            Attractions
          </Link>
        }
      />
      {quotationsTab && (
        <TabList
          label="Trips page sections"
          tabs={TABS}
          selected={tab}
          // A new tab starts without the other tab's filters, sort and page.
          onSelect={(next) => setParams(next === 'trips' ? {} : { tab: next }, { replace: true })}
        />
      )}
      <div
        role={quotationsTab ? 'tabpanel' : undefined}
        id={`panel-${tab}`}
        aria-labelledby={quotationsTab ? `tab-${tab}` : undefined}
      >
        {tab === 'quotations' && quotationsTab ? quotationsTab() : <TripRequestsList />}
      </div>
    </section>
  );
}
