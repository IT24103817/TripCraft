import { statusLabel } from '../statuses';

interface TopbarProps {
  userName: string;
  role: string;
  menuOpen: boolean;
  onToggleMenu: () => void;
  onLogout: () => void;
}

export function Topbar({ userName, role, menuOpen, onToggleMenu, onLogout }: TopbarProps) {
  return (
    <header className="flex items-center justify-between gap-2 border-b border-slate-200 bg-white px-4 py-3">
      <button
        type="button"
        className="btn-secondary md:hidden"
        aria-controls="main-navigation"
        aria-expanded={menuOpen}
        onClick={onToggleMenu}
      >
        Menu
      </button>
      <div className="ml-auto flex items-center gap-3 text-sm">
        <span className="hidden text-right sm:block">
          <span className="block font-medium text-slate-900">{userName}</span>
          <span className="block text-xs text-slate-500">{statusLabel(role)}</span>
        </span>
        <button type="button" className="btn-secondary" onClick={onLogout}>
          Log out
        </button>
      </div>
    </header>
  );
}
