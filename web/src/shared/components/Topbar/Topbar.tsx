import { NotificationBell } from '../../notifications/NotificationBell';
import { statusLabel } from '../../statuses';
import { cn } from '../../utils/cn';
import { Button } from '../Button';
import { ThemeToggle } from '../ThemeToggle';
import type { TopbarProps } from './Topbar.types';

/** "Nimal Perera" -> "NP". */
const initialsOf = (name: string) =>
  name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part.charAt(0).toUpperCase())
    .join('');

/** The bar above every staff page: menu button (mobile), notifications, theme, the signed-in user and Log out. */
export const Topbar = ({ userName, role, menuOpen, onToggleMenu, onLogout, className }: TopbarProps) => (
  <header
    className={cn(
      'flex items-center justify-between gap-2 border-b border-slate-200 bg-surface px-4 py-3',
      className,
    )}
  >
    <Button
      variant="secondary"
      className="md:hidden"
      aria-controls="main-navigation"
      aria-expanded={menuOpen}
      onClick={onToggleMenu}
    >
      <svg
        viewBox="0 0 24 24"
        className="h-4 w-4"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        aria-hidden="true"
      >
        <path d="M4 6h16M4 12h16M4 18h16" strokeLinecap="round" />
      </svg>
      Menu
    </Button>
    <div className="ml-auto flex items-center gap-3 text-sm">
      <NotificationBell />
      <ThemeToggle />
      <span className="hidden items-center gap-3 sm:flex">
        <span className="text-right">
          <span className="block font-medium text-slate-900">{userName}</span>
          <span className="block text-xs text-slate-500">{statusLabel(role)}</span>
        </span>
        <span
          className="flex h-9 w-9 items-center justify-center rounded-full bg-brand-100 text-xs font-semibold text-brand-800"
          aria-hidden="true"
        >
          {initialsOf(userName)}
        </span>
      </span>
      <Button variant="secondary" onClick={onLogout}>
        Log out
      </Button>
    </div>
  </header>
);
