import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { getErrorMessage, getErrorStatus } from '@/shared/api/errors';
import { FormField } from '@/shared/components/FormField';
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
    <main className="flex min-h-screen items-center justify-center p-4">
      <form
        noValidate
        aria-labelledby="login-title"
        className="card w-full max-w-sm space-y-4"
        onSubmit={handleSubmit((values) => mutation.mutate(values))}
      >
        <div>
          <h1 id="login-title" className="text-xl font-semibold text-slate-900">
            TripCraft operations
          </h1>
          <p className="text-sm text-slate-600">Sign in with your staff account.</p>
        </div>
        {errorMessage && (
          <p role="alert" className="rounded bg-red-50 p-2 text-sm text-red-700">
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
    </main>
  );
}
