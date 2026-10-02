export interface TopbarProps {
  userName: string;
  role: string;
  /** Whether the off-canvas sidebar is open (below 768 px). */
  menuOpen: boolean;
  onToggleMenu: () => void;
  onLogout: () => void;
  className?: string;
}
