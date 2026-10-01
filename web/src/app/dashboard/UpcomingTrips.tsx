import { Link } from 'react-router-dom';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useUpcomingTrips, type UpcomingTripDto } from './dashboardApi';

const DAYS: { day: UpcomingTripDto['day']; title: string }[] = [
  { day: 'today', title: 'Today' },
  { day: 'tomorrow', title: 'Tomorrow' },
];

/** "Today and tomorrow": confirmed or running trips, with their guide and vehicle (GET /api/dashboard/upcoming). */
export function UpcomingTrips() {
  const upcoming = useUpcomingTrips();
  const trips = upcoming.data ?? [];
  return (
    <section aria-labelledby="upcoming-trips" className="space-y-3">
      <h2 id="upcoming-trips" className="text-lg font-semibold text-slate-900">
        Today and tomorrow
      </h2>
      <PageState
        isLoading={upcoming.isLoading}
        isError={upcoming.isError}
        error={upcoming.error}
        onRetry={() => upcoming.refetch()}
        isEmpty={trips.length === 0}
        emptyTitle="No trips today or tomorrow"
        emptyDescription="Confirmed trips appear here on the day before they run."
      >
        <div className="grid gap-4 lg:grid-cols-2">
          {DAYS.map(({ day, title }) => {
            const list = trips.filter((t) => t.day === day);
            return (
              <div key={day} className="card space-y-2">
                <h3 className="font-semibold text-slate-900">{title}</h3>
                {list.length === 0 ? (
                  <p className="text-sm text-slate-600">No trips.</p>
                ) : (
                  <ul aria-label={`Trips ${day}`} className="divide-y divide-slate-200">
                    {list.map((trip) => (
                      <UpcomingTripRow key={trip.tripRequestId} trip={trip} />
                    ))}
                  </ul>
                )}
              </div>
            );
          })}
        </div>
      </PageState>
    </section>
  );
}

function UpcomingTripRow({ trip }: { trip: UpcomingTripDto }) {
  const vehicle = trip.vehicleRegistrationNo
    ? `${trip.vehicleType ?? 'Vehicle'} ${trip.vehicleRegistrationNo}`
    : 'No vehicle';
  return (
    <li className="space-y-1 py-2 text-sm">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <Link to={`/trips/${trip.tripRequestId}`} className="font-medium text-brand-700 hover:underline">
          {trip.objective}
        </Link>
        <StatusBadge status={trip.status} />
      </div>
      <p className="text-slate-600">
        Day {trip.dayNumber} · {trip.touristName} · {trip.pax} travellers
      </p>
      <p className="text-slate-700">
        Guide: {trip.guideName ?? 'none'} · {vehicle}
      </p>
    </li>
  );
}
