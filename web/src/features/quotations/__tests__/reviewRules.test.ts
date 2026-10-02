import { describe, expect, it } from 'vitest';
import { quotation } from '@/test/fixtures';
import { budgetNoteText, confirmBlockedReason, declineReason, sendBlockedReason } from '../reviewRules';
import type { QuotationDto } from '../types';

const version = (overrides: Record<string, unknown> = {}) => quotation(overrides) as unknown as QuotationDto;
const accepted = version({ version: 2, status: 'Approved', acceptedAt: '2026-09-28T10:00:00Z' });

describe('confirmBlockedReason (the API 409 rules of Confirm)', () => {
  it('allows confirming the newest version once the client accepted it', () => {
    expect(confirmBlockedReason({ newest: accepted, editedSinceQuotation: false })).toBeNull();
  });

  it('blocks when the proposal was edited since it was priced', () => {
    expect(confirmBlockedReason({ newest: accepted, editedSinceQuotation: true })).toMatch(
      /edited after it was priced/,
    );
  });

  it('blocks when the newest version is not the accepted one (a re-price made a new version)', () => {
    const repriced = version({ version: 3, status: 'Pending' });
    expect(confirmBlockedReason({ newest: repriced, editedSinceQuotation: false })).toMatch(
      /Version 3 has not been accepted by the client/,
    );
  });

  it('blocks when there is no quotation yet', () => {
    expect(confirmBlockedReason({ newest: undefined, editedSinceQuotation: false })).toMatch(/no quotation/);
  });
});

describe('sendBlockedReason (POST /api/quotations/{id}/send)', () => {
  it('allows sending a re-priced version that was not sent yet', () => {
    expect(
      sendBlockedReason({ newest: version({ status: 'Pending' }), editedSinceQuotation: false }),
    ).toBeNull();
  });

  it('blocks a proposal edited since it was priced', () => {
    expect(sendBlockedReason({ newest: version(), editedSinceQuotation: true })).toMatch(/Re-price it first/);
  });

  it('blocks a version already sent or declined', () => {
    expect(sendBlockedReason({ newest: accepted, editedSinceQuotation: false })).toMatch(
      /Version 2 is already sent to client/,
    );
    expect(
      sendBlockedReason({ newest: version({ status: 'Declined' }), editedSinceQuotation: false }),
    ).toMatch(/declined by client/);
  });

  it('blocks when nothing was priced yet', () => {
    expect(sendBlockedReason({ newest: undefined, editedSinceQuotation: false })).toMatch(/Re-price/);
  });
});

describe('declineReason', () => {
  it("returns the client's newest decline reason only for a Declined version", () => {
    const declined = version({
      status: 'Declined',
      decisions: [{ decision: 'Declined', comment: 'Too expensive', decidedAt: '2026-09-28T08:00:00Z' }],
    });
    expect(declineReason(declined)).toBe('Too expensive');
    expect(declineReason({ ...declined, status: 'Pending' })).toBeNull();
    expect(declineReason(undefined)).toBeNull();
  });
});

describe('budgetNoteText', () => {
  it("uses the API's sentence for a best-price version", () => {
    const best = version({
      bestAvailablePrice: true,
      overBudgetUsd: 132,
      budgetNote: 'Best price we can offer — USD 132.00 above your budget',
    });
    expect(budgetNoteText(best)).toBe('Best price we can offer — USD 132.00 above your budget');
  });

  it('builds the sentence from overBudgetUsd when the API sends none', () => {
    const best = version({ bestAvailablePrice: true, overBudgetUsd: 1234.5, budgetNote: null });
    expect(budgetNoteText(best)).toBe('Best price we can offer — USD 1,234.50 above your budget');
  });

  it('says nothing for a version within budget', () => {
    expect(budgetNoteText(version({ bestAvailablePrice: false, budgetNote: null }))).toBeNull();
    expect(budgetNoteText(version())).toBeNull();
  });
});
