import { PageHeader } from '@/shared/components/PageHeader';
import { AvailabilitySearch } from './AvailabilitySearch';
import { HoldCalendar } from './HoldCalendar';

/** Component B business view: search what is free and see what is held (indigo = trip, amber = manual block). */
export default function AvailabilityPage() {
  return (
    <section className="space-y-4">
      <PageHeader
        title="Availability"
        description="The same rules the Resource agent uses, and every hold."
      />
      <AvailabilitySearch />
      <HoldCalendar />
    </section>
  );
}
