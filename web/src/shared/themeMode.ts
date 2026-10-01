/** Light or dark theme. The choice is kept in localStorage; without one, the operating system's setting is used. */
export type ThemeMode = 'light' | 'dark';

export const THEME_STORAGE_KEY = 'tripcraft-theme';

/** The saved choice, or the OS preference ("prefers-color-scheme: dark"), or light. */
export function initialThemeMode(): ThemeMode {
  const saved = readSaved();
  if (saved) return saved;
  const prefersDark =
    typeof window.matchMedia === 'function' && window.matchMedia('(prefers-color-scheme: dark)').matches;
  return prefersDark ? 'dark' : 'light';
}

/** Tailwind's dark mode is the `dark` class on <html> (tailwind.config.ts: darkMode 'class'). */
export function applyThemeMode(mode: ThemeMode): void {
  document.documentElement.classList.toggle('dark', mode === 'dark');
}

export function currentThemeMode(): ThemeMode {
  return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
}

export function saveThemeMode(mode: ThemeMode): void {
  try {
    localStorage.setItem(THEME_STORAGE_KEY, mode);
  } catch {
    // Storage can be blocked (private mode): the theme still changes for this visit.
  }
}

function readSaved(): ThemeMode | null {
  try {
    const value = localStorage.getItem(THEME_STORAGE_KEY);
    return value === 'light' || value === 'dark' ? value : null;
  } catch {
    return null;
  }
}
