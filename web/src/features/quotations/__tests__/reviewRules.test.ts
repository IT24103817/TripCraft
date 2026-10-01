import { describe, expect, it } from 'vitest';
import { declineReason, sendBlockedReason } from '../reviewRules';
import type { QuotationDto } from '../types';

const ready = {
  workflowStatus: 'PendingApproval' as const,
  editedSinceQuotation: false,
  quotationId: 'q1',
  quotationStatus: 'Pending' as const,
};

describe('sendBlockedReason (the API 409 rules of Send to client)', () => {
  it('allows sending a valid, priced, pending version', () => {
    expect(sendBlockedReason(ready)).toBeNull();
    // While the stored quotation is still loading, the button is not blocked by its status.
    expect(sendBlockedReason({ ...ready, quotationStatus: undefined })).toBeNull();
  });

  it('blocks an over-budget proposal (workflow RevisionRequested)', () => {
    expect(sendBlockedReason({ ...ready, workflowStatus: 'RevisionRequested' })).toMatch(/over the budget/);
  });

  it('blocks a proposal edited since it was priced', () => {
    expect(sendBlockedReason({ ...ready, editedSinceQuotation: true })).toMatch(/Re-price it first/);
  });

  it('blocks a version the client declined, and one already sent', () => {
    expect(sendBlockedReason({ ...ready, quotationStatus: 'Declined' })).toMatch(/client declined/);
    expect(sendBlockedReason({ ...ready, quotationStatus: 'Approved' })).toMatch(/sent to client/);
  });

  it('blocks when there is no quotation yet', () => {
    expect(sendBlockedReason({ ...ready, quotationId: null })).toMatch(/no priced quotation/);
  });
});

describe('declineReason', () => {
  it("returns the client's newest decline reason only for a Declined version", () => {
    const declined = {
      status: 'Declined',
      decisions: [{ decision: 'Declined', comment: 'Too expensive', decidedAt: '2026-09-28T08:00:00Z' }],
    } as QuotationDto;
    expect(declineReason(declined)).toBe('Too expensive');
    expect(declineReason({ ...declined, status: 'Pending' })).toBeNull();
    expect(declineReason(undefined)).toBeNull();
  });
});
