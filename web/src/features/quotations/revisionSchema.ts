import { z } from 'zod';

/** Mirrors RequestRevisionRequestValidator: the comment goes to the Planner agent. */
export const revisionSchema = z.object({
  comment: z
    .string()
    .trim()
    .min(1, 'A comment is required for a revision.')
    .max(1000, 'At most 1000 characters.'),
});
