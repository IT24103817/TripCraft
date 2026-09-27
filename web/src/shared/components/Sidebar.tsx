import { NavLink } from 'react-router-dom';
import type { Role } from '../api/types';
import { cn } from '../utils/cn';
import { Logo } from './Logo';

export interface NavItem {
  to: string;
  label: string;
  roles: Role[];
}

interface SidebarProps {
  items: NavItem[];
  role: Role;
  open: boolean;
  onNavigate: () => void;
}

/** Only shows the links the current role may open (PLAN.md section 2). Off-canvas below 768 px. */
export function Sidebar({ items, role, open, onNavigate }: SidebarProps) {
  const visible = items.filter((item) => item.roles.includes(role));
  return (
    <nav
      id="main-navigation"
      aria-label="Main"
      className={cn(
        'fixed inset-y-0 left-0 z-30 w-60 transform bg-brand-950 p-4 text-brand-50 transition-transform md:static md:translate-x-0',
        open ? 'translate-x-0' : '-translate-x-full',
      )}
    >
      <Logo tone="light" className="mb-8 text-lg" />
      <ul className="space-y-1">
        {visible.map((item) => (
          <li key={item.to}>
            <NavLink
              to={item.to}
              end={item.to === '/'}
              onClick={onNavigate}
              className={({ isActive }) =>
                cn(
                  'block rounded-md px-3 py-2 text-sm focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
                  isActive
                    ? 'bg-brand-800 font-semibold text-white'
                    : 'text-brand-100 hover:bg-brand-900 hover:text-white',
                )
              }
            >
              {item.label}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}
