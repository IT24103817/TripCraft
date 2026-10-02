import { NavLink } from 'react-router-dom';
import type { Role } from '../../api/types';
import { cn } from '../../utils/cn';
import { Logo } from '../Logo';
import type { NavGroup, SidebarProps } from './Sidebar.types';

/** The groups and links the current role may open; a group with no such link is left out. */
const visibleGroups = (groups: NavGroup[], role: Role): NavGroup[] =>
  groups
    .map((group) => ({ ...group, items: group.items.filter((item) => item.roles.includes(role)) }))
    .filter((group) => group.items.length > 0);

/**
 * Only shows the links the current role may open, in their groups. The active link has an accent bar on the
 * left. Off-canvas below 768 px.
 */
export const Sidebar = ({ groups, role, open, onNavigate, className }: SidebarProps) => (
  <nav
    id="main-navigation"
    aria-label="Main"
    className={cn(
      'fixed inset-y-0 left-0 z-30 w-60 transform overflow-y-auto bg-brand-950 p-4 text-brand-50 shadow-xl transition-transform md:static md:translate-x-0 md:shadow-none',
      open ? 'translate-x-0' : '-translate-x-full',
      className,
    )}
  >
    <Logo tone="light" className="mb-8 px-3 text-lg" />
    <div className="space-y-6">
      {visibleGroups(groups, role).map((group) => {
        const headingId = `nav-group-${group.label.toLowerCase()}`;
        return (
          <section key={group.label} aria-labelledby={headingId}>
            <h2
              id={headingId}
              className="mb-2 px-3 text-xs font-semibold uppercase tracking-wide text-brand-200"
            >
              {group.label}
            </h2>
            <ul className="space-y-1">
              {group.items.map((item) => (
                <li key={item.to}>
                  <NavLink
                    to={item.to}
                    onClick={onNavigate}
                    className={({ isActive }) =>
                      cn(
                        'block rounded-md border-l-4 px-3 py-2 text-sm transition-colors duration-150 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
                        isActive
                          ? 'border-accent bg-brand-800 font-semibold text-white'
                          : 'border-transparent text-brand-100 hover:bg-brand-900 hover:text-white',
                      )
                    }
                  >
                    {item.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </section>
        );
      })}
    </div>
  </nav>
);
