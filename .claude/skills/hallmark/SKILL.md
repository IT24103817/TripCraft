---
name: hallmark
description: TripCraft's design system ("Hallmark") for the React staff app (Tailwind) and the Flutter tourist/guide app (ThemeData). Use when building, restyling or reviewing any TripCraft screen or component, so both apps share the same colours, type, spacing, radii and component patterns.
---

# Hallmark — the TripCraft design system

One visual language for both clients. React reads the tokens from `web/tailwind.config.ts` and the component
classes in `web/src/index.css`; Flutter reads them from `mobile/lib/shared/theme/app_theme.dart`. Never hard-code a
colour, font size or radius in a screen: use a token, a component class or a theme value.

## Principles

1. **Calm and trustworthy** — tourists pay real money; staff approve real bookings. Quiet neutrals, one brand
   colour, colour used for meaning (status, errors), not decoration.
2. **Status first** — every trip, workflow, quotation and hold shows its status as a badge with the same tone on
   both platforms.
3. **Every state is designed** — loading, empty, error (with Retry) and success exist for every screen that loads
   data.
4. **Accessible by default** — WCAG 2.1 AA contrast, visible focus, labels on every input, 44 px minimum touch
   targets on web and 48 dp on mobile, no information by colour alone.

## Tokens

### Colour

| Token | Value | Tailwind | Flutter (`AppColors`) | Use |
|-------|-------|----------|-----------------------|-----|
| brand-50 / 100 | `#F0FDFA` / `#CCFBF1` | `brand-50`, `brand-100` | `brandSoft` | selected rows, active nav background (light) |
| brand-500 | `#14B8A6` | `brand-500` | — | focus rings |
| brand-600 | `#0D9488` | `brand-600` | — | hover of primary |
| **brand-700** | **`#0F766E`** | `brand-700` | `brand` | primary buttons, links, active items (5.5:1 on white) |
| brand-800 / 900 / 950 | `#115E59` / `#134E4A` / `#042F2E` | `brand-800…950` | `brandDeep` (`#042F2E`) | sidebar, hero, headings on light |
| accent | `#D97706` (amber-600) | `accent` / `amber-600` | `accent` | highlights, "Get the app", map pins — never for text on white below 18 px |
| ink | `#0F172A` (slate-900) | `slate-900` | `ink` | primary text |
| muted | `#475569` (slate-600) | `slate-600` | `muted` | secondary text (7.6:1) |
| subtle | `#64748B` (slate-500) | `slate-500` | — | captions, table headers (4.8:1) |
| border | `#E2E8F0` (slate-200) | `slate-200` | `border` | card and table borders |
| canvas | `#F8FAFC` (slate-50) | `slate-50` | `canvas` | page background |
| surface | `#FFFFFF` | `white` | `surface` | cards, inputs |
| success | `#15803D` | `green-700` / bg `green-50` | `success` | Confirmed, Approved, Completed, passed checks |
| warning | `#B45309` | `amber-700` / bg `amber-50` | `warning` | Pending approval, Revision requested, stale FX |
| danger | `#B91C1C` | `red-700` / bg `red-50` | `danger` | Rejected, Failed safely, errors, delete |
| info | `#1D4ED8` | `blue-700` / bg `blue-50` | `info` | Planning, In progress |
| neutral | `#475569` | `slate-600` / bg `slate-100` | `neutral` | Submitted, Cancelled, Released |

Status → tone is defined once: `web/src/shared/statuses.ts` (`toneFor`) and `mobile/lib/shared/utils/statuses.dart`.

### Typography

Typeface **Inter** (web, Google Fonts, weights 400/500/600/700) with `system-ui` fallback; Flutter uses the platform
font (Roboto / SF Pro) with the same scale.

| Role | Size / line | Weight | Tailwind | Flutter `TextTheme` |
|------|-------------|--------|----------|---------------------|
| Display (landing hero) | 44 / 52 (32 / 40 on mobile) | 700 | `text-4xl sm:text-5xl font-bold tracking-tight` | `displaySmall` |
| Page title | 24 / 32 | 600 | `text-2xl font-semibold` | `headlineSmall` |
| Section title | 18 / 28 | 600 | `text-lg font-semibold` | `titleLarge` (20) |
| Card title | 16 / 24 | 600 | `font-semibold` | `titleMedium` |
| Body | 14 / 20 (web), 16 / 24 (mobile) | 400 | `text-sm` | `bodyLarge` / `bodyMedium` |
| Caption | 12 / 16 | 500 | `text-xs` | `bodySmall`, `labelSmall` |

Numbers in tables and money use tabular figures (`tabular-nums`).

### Spacing, radius, elevation, motion

- **Spacing** — 4 px grid: 4, 8, 12, 16, 20, 24, 32, 48, 64. Card padding 20 (web) / 16 (mobile); page gutter 24
  (web) / 16 (mobile); gap between cards 16.
- **Radius** — `sm` 6 (badges inside tables), `md` 10 (buttons, inputs), `lg` 16 (cards, dialogs), `pill` 999
  (status badges, chips). Tailwind: `rounded-md` = 10 px, `rounded-lg` = 16 px (overridden in the config).
- **Elevation** — one card shadow `0 1px 2px rgb(15 23 42 / 0.06), 0 1px 3px rgb(15 23 42 / 0.08)` (`shadow-card`);
  dialogs `shadow-xl`. Flutter cards: elevation 0 with a 1 px `border` outline.
- **Motion** — 150 ms ease-out for hover/press colour changes; no motion that carries meaning.

## Component patterns

| Component | React | Flutter | Rules |
|-----------|-------|---------|-------|
| Button | `Button` (`variant` primary / secondary / danger, `size`, `isLoading` + `loadingText`, forwards its ref); a `Link` styled as a button uses the same `.btn-*` class | `FilledButton` / `OutlinedButton` | `type` defaults to `button`; while loading it is disabled and `aria-busy` |
| Primary button | `.btn-primary` (brand-700, white text, 40 px high, `rounded-md`) | `FilledButton` (theme: brand, 48 dp, radius 10) | one per view region; verb label ("Approve", "Submit trip request") |
| Secondary button | `.btn-secondary` (white, slate-300 border) | `OutlinedButton` | |
| Danger button | `.btn-danger` (red-700) | `FilledButton` with `danger` background | only for destructive actions, always behind a confirmation |
| Input | `.input` (40 px, `rounded-md`, brand focus ring) + `FormField` label/hint/error | `AppTextField` (filled surface, radius 10, label above value) | label always visible; error text in danger below the field |
| Card | `Card` (`title` → h2 on the left, `actions` on the right) or `.card` on a semantic element (`section`, `form`) | `SectionCard` | |
| Card (class) | `.card` (white, `rounded-lg`, border, `shadow-card`, p-5) | `SectionCard` (`Card` from the theme) | title row: card title left, status badge right |
| Status badge | `StatusBadge` (pill, tone background 50, text 700, ring 300) | `StatusChip` | text label always present |
| Data table | `DataTable` (header on `slate-50`, `text-xs` uppercase subtle headers, row hover `brand-50/40`) | `ListTile`s in cards | sortable headers show ▲/▼; pagination bottom right |
| Page header | `PageHeader` (title + description left, actions right) | `AppBar` (large title, no centre) | |
| Sidebar | deep brand `brand-950`, items `text-brand-100`, active `bg-brand-800 text-white` | bottom `NavigationBar` with brand indicator | |
| Timeline | `StepTimeline` / `StatusTimeline`: 24 px dots, done = success, current = brand, todo = slate-300 | `StatusTimeline` widget, same tones | |
| Toast / snackbar | `Toast` (success = green, error = red) | `SnackBar` (floating, ink background) | |
| States | `PageState`: skeleton, empty (title + description + action), error (message + Retry) | `AsyncView`, `EmptyState` | never a blank screen |

## React component files

Every shared component lives in its own folder under `web/src/shared/components/<Name>/`:

- `<Name>.tsx` — the component: an arrow function, a named export, props typed with its interface.
- `<Name>.types.ts` — `interface <Name>Props` (and any related types); every component accepts `className`.
- `index.ts` — re-exports the component and its types. `web/src/shared/components/index.ts` re-exports every folder.

Components that wrap one native element (`Button`, `Card`) use `forwardRef`. Styling is Tailwind only: tokens from
`tailwind.config.ts` and the component classes in `index.css`, joined with `cn()`. Tests live in
`web/src/shared/components/__tests__/`.

## Accessibility rules

- Text contrast ≥ 4.5:1 (≥ 3:1 for 18 px+ or bold 14 px+). brand-700, slate-600 and the 700 status tones pass on
  white and on their 50 backgrounds.
- Focus: `focus-visible:ring-2 ring-brand-500 ring-offset-2` on every interactive element.
- Every input has a `<label>`; icons that act as buttons have `aria-label`; charts have an `sr-only` table.
- Landmarks: one `<main>`, `<nav aria-label>`, one `h1` per page; the landing page uses `header`/`section`/`footer`.

## Checklist before merging UI

- [ ] Only tokens / component classes / theme values used (no raw hex outside the config and `app_theme.dart`).
- [ ] Loading, empty, error and success states present.
- [ ] Keyboard and screen-reader pass; Lighthouse accessibility ≥ 90 on public pages.
- [ ] Looks right at 360 px wide and on a desktop.
