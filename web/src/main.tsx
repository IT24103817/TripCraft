import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './app/App';
import { applyThemeMode, initialThemeMode } from './shared/themeMode';
import '@fontsource-variable/inter';
import './index.css';

// Light or dark before the first paint: the saved choice, else the operating system's setting.
applyThemeMode(initialThemeMode());

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
