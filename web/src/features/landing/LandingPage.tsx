import { Link } from 'react-router-dom';
import { Logo } from '@/shared/components/Logo';
import { GROUP_LABEL } from './landingConfig';
import { GetTheApp, HowItWorks, WhoItsFor } from './sections';

/** Public home page (no login): what TripCraft is, how it works, who it is for and where to get the app. */
export default function LandingPage() {
  return (
    <div className="min-h-screen bg-slate-50">
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:absolute focus:left-2 focus:top-2 focus:z-50 focus:rounded-md focus:bg-surface focus:p-2"
      >
        Skip to content
      </a>
      <header className="border-b border-slate-200 bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6">
          <Link
            to="/"
            aria-label="TripCraft home"
            className="rounded-md focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
          >
            <Logo />
          </Link>
          <nav
            aria-label="Landing"
            className="hidden items-center gap-6 text-sm font-medium text-slate-700 md:flex"
          >
            <a href="#how-it-works" className="hover:text-brand-700">
              How it works
            </a>
            <a href="#who-its-for" className="hover:text-brand-700">
              Who it’s for
            </a>
            <a href="#get-the-app" className="hover:text-brand-700">
              Get the app
            </a>
          </nav>
          <Link to="/login" className="btn-primary">
            Staff login
          </Link>
        </div>
      </header>

      <main id="main">
        <section aria-labelledby="hero-title" className="bg-gradient-to-b from-brand-50 to-slate-50">
          <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-24">
            <p className="text-sm font-semibold uppercase tracking-wide text-brand-700">
              Custom Sri Lanka trips
            </p>
            <h1
              id="hero-title"
              className="mt-3 max-w-3xl text-4xl font-bold tracking-tight text-slate-900 sm:text-5xl"
            >
              Tell us the trip you want. We plan it, price it and book it for you.
            </h1>
            <p className="mt-5 max-w-2xl text-lg text-slate-600">
              Choose your dates, group size and budget in the TripCraft app. We put together the days, a local
              guide, transport and hotels — and a member of our team checks everything before you say yes.
            </p>
            <div className="mt-8 flex flex-wrap items-center gap-3">
              <a href="#get-the-app" className="btn-primary px-6 text-base">
                Get the app
              </a>
              <a href="#how-it-works" className="btn-secondary px-6 text-base">
                How it works
              </a>
            </div>
            <p className="mt-4 text-sm text-slate-600">
              Tourist? Use the app — this website is for our operations team.
            </p>
          </div>
        </section>

        <HowItWorks />
        <WhoItsFor />
        <GetTheApp />
      </main>

      <footer className="border-t border-slate-200 bg-surface">
        <div className="mx-auto flex max-w-6xl flex-col gap-2 px-4 py-6 text-sm text-slate-600 sm:flex-row sm:items-center sm:justify-between sm:px-6">
          <Logo className="text-base" />
          <p>{GROUP_LABEL}SE3090 Software Engineering Frameworks, Assignment 1 (2026)</p>
        </div>
      </footer>
    </div>
  );
}
