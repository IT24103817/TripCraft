import { cn } from '../../utils/cn';
import type { LogoProps } from './Logo.types';

/** TripCraft mark (a compass point over a route) and wordmark. `tone` picks the colours for light or dark backgrounds. */
export const Logo = ({ tone = 'dark', className }: LogoProps) => (
  <span className={cn('inline-flex items-center gap-2 font-bold tracking-tight', className)}>
    <svg viewBox="0 0 32 32" className="h-7 w-7 shrink-0" aria-hidden="true">
      <rect
        width="32"
        height="32"
        rx="9"
        className={tone === 'light' ? 'fill-brand-500' : 'fill-brand-700'}
      />
      <path d="M16 6l4.5 10L16 26l-4.5-10z" className="fill-white" />
      <circle cx="16" cy="16" r="2.2" className="fill-accent" />
    </svg>
    <span className={tone === 'light' ? 'text-white' : 'text-slate-900'}>TripCraft</span>
  </span>
);
