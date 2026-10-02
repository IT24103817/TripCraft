import { Link } from 'react-router-dom';
import { APK_URL } from './landingConfig';

const STEPS = [
  {
    title: 'Tell us your trip',
    text: 'Open the TripCraft app, pick your dates, how many of you are travelling and your budget, and add a photo of your passport.',
  },
  {
    title: 'We plan it for you',
    text: 'Our planning assistant builds a day-by-day itinerary with a guide, a vehicle and hotels, and prices it in LKR and US dollars.',
  },
  {
    title: 'Checked and sent to you',
    text: 'Every plan is checked against our rules (opening hours, driving time, free rooms and your budget), then the quotation goes straight to your app. Nothing is booked yet.',
  },
  {
    title: 'You accept, we confirm',
    text: 'Accept or decline the quotation on your phone. Once you accept, a TripCraft operations manager confirms it, books the guide, vehicle and hotels and sends your vouchers.',
  },
];

const AUDIENCES = [
  {
    title: 'Tourists',
    text: 'Plan a Sri Lanka holiday from your phone: submit a request, follow its status, see your itinerary on a map and accept your quotation.',
  },
  {
    title: 'Guides',
    text: 'See the trips you are assigned to, with hotels and stops for each day, and check in at every stop with your phone’s GPS.',
  },
  {
    title: 'Operations',
    text: 'Manage guides, vehicles and hotels, see why each AI-drafted plan was chosen, confirm accepted quotations, step in when the agents need help and follow the reports.',
  },
];

export function HowItWorks() {
  return (
    <section id="how-it-works" aria-labelledby="how-title" className="bg-surface py-16 sm:py-20">
      <div className="mx-auto max-w-6xl px-4 sm:px-6">
        <h2 id="how-title" className="text-3xl font-bold tracking-tight text-slate-900">
          How it works
        </h2>
        <p className="mt-2 max-w-2xl text-slate-600">Four steps from an idea to a confirmed trip.</p>
        <ol className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {STEPS.map((step, i) => (
            <li key={step.title} className="card">
              <span
                aria-hidden="true"
                className="flex h-9 w-9 items-center justify-center rounded-full bg-brand-700 font-semibold text-white"
              >
                {i + 1}
              </span>
              <h3 className="mt-4 font-semibold text-slate-900">{step.title}</h3>
              <p className="mt-2 text-sm text-slate-600">{step.text}</p>
            </li>
          ))}
        </ol>
      </div>
    </section>
  );
}

export function WhoItsFor() {
  return (
    <section id="who-its-for" aria-labelledby="who-title" className="py-16 sm:py-20">
      <div className="mx-auto max-w-6xl px-4 sm:px-6">
        <h2 id="who-title" className="text-3xl font-bold tracking-tight text-slate-900">
          Who it’s for
        </h2>
        <ul className="mt-10 grid gap-6 md:grid-cols-3">
          {AUDIENCES.map((audience) => (
            <li key={audience.title} className="card border-t-4 border-t-brand-700">
              <h3 className="text-lg font-semibold text-slate-900">{audience.title}</h3>
              <p className="mt-2 text-sm text-slate-600">{audience.text}</p>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}

export function GetTheApp() {
  return (
    <section id="get-the-app" aria-labelledby="app-title" className="bg-brand-950 py-16 text-white sm:py-20">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 sm:px-6 md:flex-row md:items-center md:justify-between">
        <div className="max-w-xl">
          <h2 id="app-title" className="text-3xl font-bold tracking-tight">
            Get the app
          </h2>
          <p className="mt-3 text-brand-100">
            The TripCraft app for Android is where tourists plan trips and guides run them. An iOS build is
            available on request.
          </p>
        </div>
        <div className="flex flex-col items-start gap-3">
          {APK_URL ? (
            <a
              href={APK_URL}
              className="btn bg-accent px-6 text-base text-slate-950 hover:bg-amber-500 focus-visible:ring-accent focus-visible:ring-offset-brand-950"
            >
              Download APK
            </a>
          ) : (
            <p className="rounded-md bg-brand-900 px-4 py-3 text-sm text-brand-100">
              The Android download link will appear here soon.
            </p>
          )}
          <Link
            to="/login"
            className="text-sm font-semibold text-brand-100 underline-offset-4 hover:text-white hover:underline"
          >
            TripCraft staff? Sign in to the operations dashboard
          </Link>
        </div>
      </div>
    </section>
  );
}
