import { useState } from 'react';
import { applyThemeMode, currentThemeMode, saveThemeMode } from '../../themeMode';
import { Button } from '../Button';
import type { ThemeToggleProps } from './ThemeToggle.types';

/** Switches between the light and the dark theme and remembers the choice in this browser. */
export const ThemeToggle = ({ className }: ThemeToggleProps) => {
  const [dark, setDark] = useState(() => currentThemeMode() === 'dark');

  const toggle = () => {
    const mode = dark ? 'light' : 'dark';
    applyThemeMode(mode);
    saveThemeMode(mode);
    setDark(!dark);
  };

  return (
    <Button variant="secondary" className={className} aria-pressed={dark} onClick={toggle}>
      <span aria-hidden="true">{dark ? '☾' : '☀'}</span>
      <span className="sr-only sm:not-sr-only">Dark mode</span>
    </Button>
  );
};
