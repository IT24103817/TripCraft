import axios from 'axios';
import type { ProblemDetails } from './types';

/** A readable message from an API error: ProblemDetails detail, first validation error, or a fallback. */
export function getErrorMessage(
  error: unknown,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    const body = error.response?.data;
    const firstFieldError = body?.errors ? Object.values(body.errors).flat()[0] : undefined;
    if (error.response?.status === 429) return 'Too many attempts. Wait a minute and try again.';
    return (
      body?.detail ||
      firstFieldError ||
      body?.title ||
      (error.response ? fallback : 'Cannot reach the server.')
    );
  }
  return error instanceof Error ? error.message : fallback;
}

export function getErrorStatus(error: unknown): number | undefined {
  return axios.isAxiosError(error) ? error.response?.status : undefined;
}
