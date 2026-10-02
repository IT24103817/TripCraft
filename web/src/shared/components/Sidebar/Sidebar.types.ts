import type { Role } from '../../api/types';

export interface NavItem {
  to: string;
  label: string;
  /** The roles that may open the link; other roles do not see it. */
  roles: Role[];
}

/** A titled block of links (Operations, Resources, Insights, Admin). */
export interface NavGroup {
  label: string;
  items: NavItem[];
}

export interface SidebarProps {
  groups: NavGroup[];
  role: Role;
  /** Below 768 px the sidebar is off-canvas; `open` slides it in. */
  open: boolean;
  onNavigate: () => void;
  className?: string;
}
