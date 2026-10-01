import type { Config } from 'tailwindcss';
import defaultTheme from 'tailwindcss/defaultTheme';
import plugin from 'tailwindcss/plugin';

/**
 * The neutral (slate) scale as "R G B" channels. Screens use slate-50 … slate-950 as usual; in dark mode the
 * scale is turned round (slate-900 ink becomes a light colour, slate-50 canvas becomes a dark one), so every
 * screen gets a readable dark theme without a second set of class names.
 */
const LIGHT_NEUTRALS = {
  '--slate-50': '248 250 252',
  '--slate-100': '241 245 249',
  '--slate-200': '226 232 240',
  '--slate-300': '203 213 225',
  '--slate-400': '148 163 184',
  '--slate-500': '100 116 139',
  '--slate-600': '71 85 105',
  '--slate-700': '51 65 85',
  '--slate-800': '30 41 59',
  '--slate-900': '15 23 42',
  '--slate-950': '2 6 23',
  '--surface': '255 255 255',
};

const DARK_NEUTRALS = {
  '--slate-50': '2 6 23',
  '--slate-100': '30 41 59',
  '--slate-200': '51 65 85',
  '--slate-300': '71 85 105',
  '--slate-400': '100 116 139',
  '--slate-500': '148 163 184',
  '--slate-600': '203 213 225',
  '--slate-700': '226 232 240',
  '--slate-800': '241 245 249',
  '--slate-900': '248 250 252',
  '--slate-950': '255 255 255',
  '--surface': '15 23 42',
};

const fromVariable = (name: string) => `rgb(var(${name}) / <alpha-value>)`;
const SLATE_STEPS = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];

/**
 * Hallmark design tokens (.claude/skills/hallmark/SKILL.md). Screens use these names, never raw colours.
 * brand = Ceylon teal (brand-700 is the primary colour), accent = saffron for highlights.
 * Dark mode: the `dark` class on <html> (ThemeToggle in the top bar).
 */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#F0FDFA',
          100: '#CCFBF1',
          200: '#99F6E4',
          300: '#5EEAD4',
          400: '#2DD4BF',
          500: '#14B8A6',
          600: '#0D9488',
          700: '#0F766E',
          800: '#115E59',
          900: '#134E4A',
          950: '#042F2E',
        },
        accent: { DEFAULT: '#D97706', soft: '#FEF3C7' },
        slate: Object.fromEntries(SLATE_STEPS.map((step) => [step, fromVariable(`--slate-${step}`)])),
        /** Cards, inputs, dialogs: white in light mode, deep slate in dark mode. */
        surface: fromVariable('--surface'),
        /** Backdrops behind dialogs and the mobile menu: always dark, in both themes. */
        ink: '#0F172A',
      },
      fontFamily: {
        sans: ['"Inter Variable"', 'Inter', ...defaultTheme.fontFamily.sans],
      },
      borderRadius: {
        md: '10px',
        lg: '16px',
      },
      boxShadow: {
        card: '0 1px 2px rgb(15 23 42 / 0.06), 0 1px 3px rgb(15 23 42 / 0.08)',
      },
    },
  },
  plugins: [
    plugin(({ addBase }) => {
      addBase({
        ':root': { ...LIGHT_NEUTRALS, colorScheme: 'light' },
        '.dark': { ...DARK_NEUTRALS, colorScheme: 'dark' },
      });
    }),
  ],
} satisfies Config;
