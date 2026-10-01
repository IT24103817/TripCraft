import { screen } from '@testing-library/react';
import { afterEach, describe, expect, it } from 'vitest';
import { THEME_STORAGE_KEY, applyThemeMode, initialThemeMode } from '@/shared/themeMode';
import { renderApp, signInAs } from '@/test/render';

describe('Dark mode', () => {
  afterEach(() => {
    localStorage.clear();
    applyThemeMode('light');
  });

  it('switches the theme from the top bar and remembers the choice', async () => {
    signInAs('OperationsManager');
    const { user } = renderApp('/dashboard');

    const toggle = await screen.findByRole('button', { name: 'Dark mode' });
    expect(toggle).toHaveAttribute('aria-pressed', 'false');
    await user.click(toggle);

    expect(document.documentElement).toHaveClass('dark');
    expect(toggle).toHaveAttribute('aria-pressed', 'true');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark');

    await user.click(toggle);
    expect(document.documentElement).not.toHaveClass('dark');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('light');
  });

  it('starts from the saved choice, else the operating system setting', () => {
    expect(initialThemeMode()).toBe('light'); // jsdom has no prefers-color-scheme
    localStorage.setItem(THEME_STORAGE_KEY, 'dark');
    expect(initialThemeMode()).toBe('dark');

    const original = window.matchMedia;
    localStorage.clear();
    window.matchMedia = ((query: string) => ({
      matches: query.includes('dark'),
    })) as typeof window.matchMedia;
    expect(initialThemeMode()).toBe('dark');
    window.matchMedia = original;
  });
});
