/**
 * Hallmark colours for code that cannot use Tailwind classes (chart libraries). Keep in step with
 * tailwind.config.ts and .claude/skills/hallmark/SKILL.md.
 */
export const CHART_COLORS = {
  primary: '#0F766E', // brand-700
  accent: '#D97706', // accent
  grid: '#94A3B866', // slate-400 at 40%: light enough on white, quiet on the dark surface
} as const;
