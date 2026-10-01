import { useQuery } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';

/** GET /api/dashboard/actions (manager): how many things wait for the manager, per kind. */
export interface DashboardActionsDto {
  proposalsToReview: number;
  clientAcceptedToConfirm: number;
  guideChangeRequests: number;
  recentCancellations: number;
  declinedQuotations: number;
}

/** One trip running today or tomorrow (GET /api/dashboard/upcoming). */
export interface UpcomingTripDto {
  tripRequestId: string;
  objective: string;
  startDate: string;
  endDate: string;
  pax: number;
  status: string;
  touristName: string;
  guideName: string | null;
  vehicleRegistrationNo: string | null;
  vehicleType: string | null;
  day: 'today' | 'tomorrow';
  dayNumber: number;
}

// Under the trips root, so a review decision or a cancellation (which refreshes trips) refreshes these too.
const DASHBOARD = [queryRoots.trips, 'dashboard'] as const;

export function useDashboardActions() {
  return useQuery({
    queryKey: [...DASHBOARD, 'actions'],
    queryFn: async () => (await http.get<DashboardActionsDto>('/api/dashboard/actions')).data,
  });
}

export function useUpcomingTrips() {
  return useQuery({
    queryKey: [...DASHBOARD, 'upcoming'],
    queryFn: async () => (await http.get<UpcomingTripDto[]>('/api/dashboard/upcoming')).data,
  });
}
