import { TripQuotationTab } from '@/features/quotations/TripQuotationTab';
import TripDetailPage from '@/features/trips/TripDetailPage';

/**
 * The trip page with its Quotation tab. Features never import each other, so the app puts the two together:
 * the trips feature draws the page and the quotations feature draws the tab.
 */
export default function TripDetailRoute() {
  return (
    <TripDetailPage quotationTab={(trip) => <TripQuotationTab tripId={trip.id} tripStatus={trip.status} />} />
  );
}
