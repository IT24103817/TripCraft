import { TripQuotationsList } from '@/features/quotations/TripQuotationsList';
import TripsListPage from '@/features/trips/TripsListPage';

/**
 * The Trips page with its Quotations tab. Features never import each other, so the app puts the two together:
 * the trips feature draws the page and the quotations feature draws the tab.
 */
export default function TripsListRoute() {
  return <TripsListPage quotationsTab={() => <TripQuotationsList />} />;
}
