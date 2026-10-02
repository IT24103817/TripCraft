import { lazy, Suspense, type ReactNode } from 'react';
import { createBrowserRouter, type RouteObject } from 'react-router-dom';
import LoginPage from '@/auth/LoginPage';
import { ProtectedRoute } from '@/auth/ProtectedRoute';
import { RoleGuard } from '@/auth/RoleGuard';
import { LoadingSkeleton } from '@/shared/components/PageState';
import type { Role } from '@/shared/api/types';
import { AppLayout } from './AppLayout';
import { QuotationsRedirect, WorkflowsRedirect } from './redirects';

const LandingPage = lazy(() => import('@/features/landing/LandingPage'));
const DashboardPage = lazy(() => import('./dashboard/DashboardPage'));
const NotFoundPage = lazy(() => import('./NotFoundPage'));
const MobileAppPage = lazy(() => import('@/auth/MobileAppPage'));
const UsersPage = lazy(() => import('@/auth/users/UsersPage'));
const AuditLogPage = lazy(() => import('@/auth/audit/AuditLogPage'));
const SettingsPage = lazy(() => import('@/auth/settings/SettingsPage'));
const TripsListRoute = lazy(() => import('./trips/TripsListRoute'));
const TripDetailRoute = lazy(() => import('./trips/TripDetailRoute'));
const AttractionsPage = lazy(() => import('@/features/trips/AttractionsPage'));
const GuidesPage = lazy(() => import('@/features/resources/GuidesPage'));
const VehiclesPage = lazy(() => import('@/features/resources/VehiclesPage'));
const HotelsPage = lazy(() => import('@/features/resources/HotelsPage'));
const AvailabilityPage = lazy(() => import('@/features/resources/AvailabilityPage'));
const ApprovalsPage = lazy(() => import('@/features/quotations/ApprovalsPage'));
const ApprovalReviewPage = lazy(() => import('@/features/quotations/ApprovalReviewPage'));
const AgentRunsPage = lazy(() => import('@/features/quotations/AgentRunsPage'));
const AgentRunDetailPage = lazy(() => import('@/features/quotations/AgentRunDetailPage'));
const ReportsPage = lazy(() => import('@/features/quotations/ReportsPage'));

const MANAGER: Role[] = ['OperationsManager'];
const STAFF: Role[] = ['OperationsManager', 'Admin'];
const ADMIN: Role[] = ['Admin'];

const guard = (roles: Role[], element: ReactNode) => <RoleGuard roles={roles}>{element}</RoleGuard>;

export const routes: RouteObject[] = [
  {
    // Public home page: no login needed.
    path: '/',
    element: (
      <Suspense fallback={<LoadingSkeleton />}>
        <LandingPage />
      </Suspense>
    ),
  },
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
          // Operations
          { path: 'dashboard', element: guard(STAFF, <DashboardPage />) },
          { path: 'trips', element: guard(MANAGER, <TripsListRoute />) },
          { path: 'trips/:id', element: guard(MANAGER, <TripDetailRoute />) },
          { path: 'attractions', element: guard(MANAGER, <AttractionsPage />) },
          { path: 'approvals', element: guard(MANAGER, <ApprovalsPage />) },
          { path: 'approvals/:id', element: guard(MANAGER, <ApprovalReviewPage />) },
          // Resources
          { path: 'resources/guides', element: guard(MANAGER, <GuidesPage />) },
          { path: 'resources/vehicles', element: guard(MANAGER, <VehiclesPage />) },
          { path: 'resources/hotels', element: guard(MANAGER, <HotelsPage />) },
          { path: 'availability', element: guard(MANAGER, <AvailabilityPage />) },
          // Insights
          { path: 'reports', element: guard(MANAGER, <ReportsPage />) },
          { path: 'agent-runs', element: guard(STAFF, <AgentRunsPage />) },
          { path: 'agent-runs/:id', element: guard(STAFF, <AgentRunDetailPage />) },
          // Admin
          { path: 'admin/users', element: guard(ADMIN, <UsersPage />) },
          { path: 'admin/audit-logs', element: guard(ADMIN, <AuditLogPage />) },
          { path: 'admin/settings', element: guard(ADMIN, <SettingsPage />) },
          // Old addresses, kept so bookmarks and links still work.
          { path: 'workflows', element: <WorkflowsRedirect /> },
          { path: 'workflows/:id', element: <WorkflowsRedirect /> },
          { path: 'quotations', element: <QuotationsRedirect /> },
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
