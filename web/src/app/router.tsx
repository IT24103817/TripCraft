import { lazy, Suspense, type ReactNode } from 'react';
import { createBrowserRouter, type RouteObject } from 'react-router-dom';
import LoginPage from '@/auth/LoginPage';
import { ProtectedRoute } from '@/auth/ProtectedRoute';
import { RoleGuard } from '@/auth/RoleGuard';
import { LoadingSkeleton } from '@/shared/components/PageState';
import type { Role } from '@/shared/api/types';
import { AppLayout } from './AppLayout';

const DashboardPage = lazy(() => import('./dashboard/DashboardPage'));
const NotFoundPage = lazy(() => import('./NotFoundPage'));
const MobileAppPage = lazy(() => import('@/auth/MobileAppPage'));
const UsersPage = lazy(() => import('@/auth/users/UsersPage'));
const TripsListPage = lazy(() => import('@/features/trips/TripsListPage'));
const TripDetailPage = lazy(() => import('@/features/trips/TripDetailPage'));
const AttractionsPage = lazy(() => import('@/features/trips/AttractionsPage'));
const GuidesPage = lazy(() => import('@/features/resources/GuidesPage'));
const VehiclesPage = lazy(() => import('@/features/resources/VehiclesPage'));
const HotelsPage = lazy(() => import('@/features/resources/HotelsPage'));
const AvailabilityPage = lazy(() => import('@/features/resources/AvailabilityPage'));
const ApprovalsPage = lazy(() => import('@/features/quotations/ApprovalsPage'));
const ApprovalReviewPage = lazy(() => import('@/features/quotations/ApprovalReviewPage'));
const WorkflowsPage = lazy(() => import('@/features/quotations/WorkflowsPage'));
const WorkflowDetailPage = lazy(() => import('@/features/quotations/WorkflowDetailPage'));
const ReportsPage = lazy(() => import('@/features/quotations/ReportsPage'));

const MANAGER: Role[] = ['OperationsManager'];
const STAFF: Role[] = ['OperationsManager', 'Admin'];
const ADMIN: Role[] = ['Admin'];

const guard = (roles: Role[], element: ReactNode) => <RoleGuard roles={roles}>{element}</RoleGuard>;

export const routes: RouteObject[] = [
  { path: '/login', element: <LoginPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/mobile-app',
        // Outside AppLayout, so it needs its own Suspense boundary for the lazy chunk.
        element: (
          <Suspense fallback={<LoadingSkeleton />}>
            <MobileAppPage />
          </Suspense>
        ),
      },
      {
        element: <AppLayout />,
        children: [
          { index: true, element: guard(STAFF, <DashboardPage />) },
          { path: 'trips', element: guard(MANAGER, <TripsListPage />) },
          { path: 'trips/:id', element: guard(MANAGER, <TripDetailPage />) },
          { path: 'attractions', element: guard(MANAGER, <AttractionsPage />) },
          { path: 'resources/guides', element: guard(MANAGER, <GuidesPage />) },
          { path: 'resources/vehicles', element: guard(MANAGER, <VehiclesPage />) },
          { path: 'resources/hotels', element: guard(MANAGER, <HotelsPage />) },
          { path: 'availability', element: guard(MANAGER, <AvailabilityPage />) },
          { path: 'approvals', element: guard(MANAGER, <ApprovalsPage />) },
          { path: 'approvals/:id', element: guard(MANAGER, <ApprovalReviewPage />) },
          { path: 'workflows', element: guard(STAFF, <WorkflowsPage />) },
          { path: 'workflows/:id', element: guard(STAFF, <WorkflowDetailPage />) },
          { path: 'reports', element: guard(MANAGER, <ReportsPage />) },
          { path: 'admin/users', element: guard(ADMIN, <UsersPage />) },
          { path: '*', element: <NotFoundPage /> },
        ],
      },
    ],
  },
];

export const router = createBrowserRouter(routes, {
  future: {
    v7_relativeSplatPath: true,
    v7_fetcherPersist: true,
    v7_normalizeFormMethod: true,
    v7_partialHydration: true,
    v7_skipActionErrorRevalidation: true,
  },
});
