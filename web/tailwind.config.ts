import type { Config } from 'tailwindcss';
import defaultTheme from 'tailwindcss/defaultTheme';

/**
 * Hallmark design tokens (.claude/skills/hallmark/SKILL.md). Screens use these names, never raw colours.
 * brand = Ceylon teal (brand-700 is the primary colour), accent = saffron for highlights.
 */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
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
  plugins: [],
} satisfies Config;
