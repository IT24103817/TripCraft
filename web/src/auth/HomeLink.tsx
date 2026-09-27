import { Link } from 'react-router-dom';
import { useAuthStore } from './authStore';
import { homeFor, isStaff } from './roles';

/**
 * "Go home" link for the 403 and 404 pages. "/" is the public landing page, so a signed-in user goes
 * to their own start page (staff: the dashboard, tourists and guides: the mobile-app page).
 */
export function HomeLink() {
  const role = useAuthStore((s) => s.user?.role);

  if (!role) {
    return (
      <Link to="/" className="text-brand-700 underline">
        Go to the TripCraft home page
      </Link>
    );
  }
  return (
    <Link to={homeFor(role)} className="text-brand-700 underline">
      {isStaff(role) ? 'Go to the dashboard' : 'Go to your start page'}
    </Link>
  );
}
