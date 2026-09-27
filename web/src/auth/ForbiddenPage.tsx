import { HomeLink } from './HomeLink';

export function ForbiddenPage() {
  return (
    <section className="card mx-auto mt-10 max-w-md space-y-2 text-center">
      <p className="text-3xl font-bold text-slate-900">403</p>
      <h1 className="text-lg font-semibold">You do not have access to this page</h1>
      <p className="text-sm text-slate-600">
        Your role cannot open this screen. Ask an administrator if you think this is wrong.
      </p>
      <HomeLink />
    </section>
  );
}
