import { HomeLink } from '@/auth/HomeLink';

export default function NotFoundPage() {
  return (
    <section className="card mx-auto mt-10 max-w-md space-y-2 text-center">
      <p className="text-3xl font-bold">404</p>
      <h1 className="text-lg font-semibold">Page not found</h1>
      <HomeLink />
    </section>
  );
}
