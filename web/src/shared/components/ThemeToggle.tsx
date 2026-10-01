import { useState } from 'react';
import { applyThemeMode, currentThemeMode, saveThemeMode } from '../themeMode';

/** Switches between the light and the dark theme and remembers the choice in this browser. */
export function ThemeToggle() {
  const [dark, setDark] = useState(() => currentThemeMode() === 'dark');

  const toggle = () => {
    const mode = dark ? 'light' : 'dark';
    applyThemeMode(mode);
    saveThemeMode(mode);
    setDark(!dark);
  };

  return (
    <button type="button" className="btn-secondary" aria-pressed={dark} onClick={toggle}>
      <span aria-hidden="true">{dark ? '☾' : '☀'}</span>
      <span className="sr-only sm:not-sr-only">Dark mode</span>
    </button>
  );
}
