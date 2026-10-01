import { describe, expect, it } from 'vitest';
import { statusLabel, toneFor, TRIP_STATUSES } from '../statuses';

describe('trip statuses (v1.1 lifecycle)', () => {
  it('lists the v1.1 trip statuses and none of the old ones', () => {
    expect(TRIP_STATUSES).toEqual([
      'Submitted',
      'Planning',
      'PendingReview',
      'QuotationSent',
      'ClientAccepted',
      'Confirmed',
      'InProgress',
      'Completed',
      'Cancelled',
      'RevisionRequested',
      'FailedSafely',
    ]);
    expect(TRIP_STATUSES).not.toContain('PendingApproval');
    expect(TRIP_STATUSES).not.toContain('Approved');
    expect(TRIP_STATUSES).not.toContain('Rejected');
  });

  it.each([
    ['PendingReview', 'Pending review'],
    ['QuotationSent', 'Quotation sent'],
    ['ClientAccepted', 'Client accepted'],
    ['FailedSafely', 'Failed safely'],
    ['RevisionRequested', 'Revision requested'],
    ['InProgress', 'In progress'],
  ])('labels %s as "%s"', (status, label) => {
    expect(statusLabel(status)).toBe(label);
  });

  it('gives every trip status a colour, and red to failures', () => {
    expect(toneFor('PendingReview')).toBe('amber');
    expect(toneFor('Confirmed')).toBe('green');
    expect(toneFor('FailedSafely')).toBe('red');
    expect(toneFor('Declined')).toBe('red');
    expect(toneFor('SomethingNew')).toBe('grey');
  });
});
