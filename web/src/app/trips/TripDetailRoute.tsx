import { TripActions } from '@/features/quotations/TripActions';
import { TripQuotationTab } from '@/features/quotations/TripQuotationTab';
import TripDetailPage from '@/features/trips/TripDetailPage';

/**
 * The trip page with its next-step actions and its Quotation tab. Features never import each other, so the app
 * puts them together: the trips feature draws the page, the quotations feature the actions and the tab.
 */
export default function TripDetailRoute() {
  return (
    <TripDetailPage
      statusActions={(trip, openCancel) => <TripActions trip={trip} onCancel={openCancel} />}
      quotationTab={(trip) => <TripQuotationTab tripId={trip.id} tripStatus={trip.status} />}
    />
  );
}
