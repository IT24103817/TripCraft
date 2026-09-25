import { z } from 'zod';
import { ROLES } from '@/shared/api/types';

/** Mirrors CreateUserRequestValidator + PasswordRules in the API. */
export const createUserSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.').max(256),
  fullName: z.string().trim().min(1, 'Full name is required.').max(200),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters.')
    .regex(/[A-Z]/, 'Password must contain an upper-case letter.')
    .regex(/[a-z]/, 'Password must contain a lower-case letter.')
    .regex(/[0-9]/, 'Password must contain a digit.'),
  role: z.enum(ROLES as [string, ...string[]]),
});

export type CreateUserForm = z.infer<typeof createUserSchema>;
