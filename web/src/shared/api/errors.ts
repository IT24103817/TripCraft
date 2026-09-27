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

/**
 * First validation message for one request field from a 400 ProblemDetails `errors` map. The API uses
 * C# property names, so "attractionIds" matches "AttractionIds" and "AttractionIds[0]".
 */
export function getFieldError(error: unknown, field: string): string | undefined {
  if (!axios.isAxiosError<ProblemDetails>(error)) return undefined;
  const errors = error.response?.data?.errors ?? {};
  const name = field.toLowerCase();
  const key = Object.keys(errors).find(
    (k) => k.toLowerCase() === name || k.toLowerCase().startsWith(`${name}[`),
  );
  return key ? errors[key]?.[0] : undefined;
}
