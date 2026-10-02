import { Navigate, useLocation, useParams, useSearchParams } from 'react-router-dom';

/** Old links (/workflows and /workflows/:id) go to the Agent runs pages, keeping any filters in the URL. */
export function WorkflowsRedirect() {
  const { id } = useParams();
  const { search } = useLocation();
  const target = id ? `/agent-runs/${id}` : '/agent-runs';
  return <Navigate to={`${target}${search}`} replace />;
}

/**
 * The Quotations page is now the Quotation tab of each trip. /quotations?tripRequestId=… opens that tab;
 * a plain /quotations opens the Quotations tab of the Trips page.
 */
export function QuotationsRedirect() {
  const [params] = useSearchParams();
  const tripId = params.get('tripRequestId');
  return <Navigate to={tripId ? `/trips/${tripId}?tab=quotation` : '/trips?tab=quotations'} replace />;
}
