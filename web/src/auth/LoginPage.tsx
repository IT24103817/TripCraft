import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { getErrorMessage, getErrorStatus } from '@/shared/api/errors';
import { FormField } from '@/shared/components/FormField';
import { Logo } from '@/shared/components/Logo';
import { login } from './authApi';
import { isSessionValid, useAuthStore } from './authStore';
import { homeFor, isStaff } from './roles';
import { loginSchema, type LoginForm } from './loginSchema';

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const store = useAuthStore();
  const from = (location.state as { from?: string } | null)?.from;

  const { register, handleSubmit, formState } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) });
  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (response) => {
      store.login(response);
      const role = response.user.role;
      navigate(isStaff(role) && from ? from : homeFor(role), { replace: true });
    },
  });

  if (isSessionValid(store) && store.user) return <Navigate to={homeFor(store.user.role)} replace />;

  const errorMessage = mutation.isError
    ? getErrorStatus(mutation.error) === 401
      ? 'Invalid email or password.'
      : getErrorMessage(mutation.error)
    : null;

  return (
    <main className="grid min-h-screen lg:grid-cols-2">
      <section
        aria-label="About TripCraft operations"
        className="hidden flex-col justify-between bg-brand-950 p-12 text-white lg:flex"
      >
        <Logo tone="light" className="text-xl" />
        <div>
          <p className="text-3xl font-semibold leading-tight tracking-tight">
            Plan, approve and run custom Sri Lanka tours.
          </p>
          <p className="mt-4 max-w-md text-brand-100">
            AI agents draft every itinerary and quotation; your team checks it and approves it before anything
            is booked.
          </p>
        </div>
        <p className="text-sm text-brand-200">Operations dashboard for staff</p>
      </section>
      <div className="flex flex-col items-center justify-center gap-6 p-4 sm:p-8">
        <Logo className="text-xl lg:hidden" />
        <form
          noValidate
          aria-labelledby="login-title"
          className="card w-full max-w-sm space-y-5 p-8"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <div>
            <h1 id="login-title" className="text-2xl font-semibold text-slate-900">
              TripCraft operations
            </h1>
            <p className="mt-1 text-sm text-slate-600">Sign in with your staff account.</p>
          </div>
          {errorMessage && (
            <p role="alert" className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              {errorMessage}
            </p>
          )}
          <FormField
            label="Email"
            type="email"
            autoComplete="username"
            registration={register('email')}
            error={formState.errors.email?.message}
          />
          <FormField
            label="Password"
            type="password"
            autoComplete="current-password"
            registration={register('password')}
            error={formState.errors.password?.message}
          />
          <button type="submit" className="btn-primary w-full" disabled={mutation.isPending}>
            {mutation.isPending ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
        <Link to="/" className="text-sm font-medium text-brand-700 hover:underline">
          Back to the TripCraft home page
        </Link>
      </div>
    </main>
  );
}
