import { Suspense, useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/auth/authStore';
import { LoadingSkeleton } from '@/shared/components/PageState';
import { Sidebar } from '@/shared/components/Sidebar';
import { Topbar } from '@/shared/components/Topbar';
import { NAV_GROUPS } from './navigation';

export function AppLayout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  if (!user) return null;

  return (
    <div className="flex min-h-screen">
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:absolute focus:left-2 focus:top-2 focus:z-50 focus:bg-surface focus:p-2"
      >
        Skip to content
      </a>
      <Sidebar groups={NAV_GROUPS} role={user.role} open={menuOpen} onNavigate={() => setMenuOpen(false)} />
      {menuOpen && (
        <button
          type="button"
          aria-label="Close menu"
          className="fixed inset-0 z-20 bg-ink/30 md:hidden"
          onClick={() => setMenuOpen(false)}
        />
      )}
      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar
          userName={user.fullName}
          role={user.role}
          menuOpen={menuOpen}
          onToggleMenu={() => setMenuOpen((open) => !open)}
          onLogout={() => {
            logout();
            navigate('/login', { replace: true });
          }}
        />
        <main id="main" className="flex-1 p-4 md:p-6 xl:mx-auto xl:w-full xl:max-w-7xl">
          <Suspense fallback={<LoadingSkeleton />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  );
}
