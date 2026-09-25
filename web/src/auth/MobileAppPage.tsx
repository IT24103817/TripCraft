import { useAuthStore } from './authStore';

/** Tourists and Guides are signed in but use the Flutter app (PLAN.md section 2). */
export default function MobileAppPage() {
  const { user, logout } = useAuthStore();
  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <section className="card max-w-md space-y-3 text-center">
        <h1 className="text-lg font-semibold text-slate-900">Please use the TripCraft mobile app</h1>
        <p className="text-sm text-slate-600">
          Hi {user?.fullName ?? 'there'} — this website is for operations staff. Tourists and guides manage
          trips in the TripCraft app for Android.
        </p>
        <button type="button" className="btn-secondary" onClick={logout}>
          Log out
        </button>
      </section>
    </main>
  );
}
