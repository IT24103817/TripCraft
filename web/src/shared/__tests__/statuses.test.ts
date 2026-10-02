import { describe, expect, it } from 'vitest';
import { statusLabel, toneFor, TRIP_STATUSES } from '../statuses';

describe('trip statuses (v1.1 lifecycle)', () => {
  it('lists the v1.1 trip statuses and none of the removed ones', () => {
    expect(TRIP_STATUSES).toEqual([
      'Submitted',
      'Planning',
      'QuotationSent',
      'ClientAccepted',
      'Confirmed',
      'InProgress',
      'Completed',
      'ClientDeclined',
      'NeedsOperator',
      'Cancelled',
    ]);
    for (const removed of ['PendingReview', 'RevisionRequested', 'FailedSafely', 'PendingApproval']) {
      expect(TRIP_STATUSES).not.toContain(removed);
    }
  });

  it.each([
    ['QuotationSent', 'Quotation sent'],
    ['ClientAccepted', 'Accepted'],
    ['ClientDeclined', 'Declined'],
    ['NeedsOperator', 'Needs operator'],
    ['InProgress', 'In progress'],
    // Agent workflow statuses are unchanged.
    ['FailedSafely', 'Failed safely'],
    ['PendingApproval', 'Pending approval'],
  ])('labels %s as "%s"', (status, label) => {
    expect(statusLabel(status)).toBe(label);
  });

  it('gives every trip status a colour: red when the operator is needed, amber after a decline', () => {
    expect(toneFor('NeedsOperator')).toBe('red');
    expect(toneFor('ClientDeclined')).toBe('amber');
    expect(toneFor('ClientAccepted')).toBe('purple');
    expect(toneFor('Confirmed')).toBe('green');
    expect(toneFor('Declined')).toBe('red');
    expect(toneFor('SomethingNew')).toBe('grey');
  });
});
